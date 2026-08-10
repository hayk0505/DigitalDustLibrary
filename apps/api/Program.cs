using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Endpoints;
using DigitalDustLibrary.Api.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Resend;

var builder = WebApplication.CreateBuilder(args);

// --- JSON: camelCase properties + snake_case enums, matching
// apps/admin/src/lib/types.ts exactly (accessToken, bodyHtml, pending_review,
// social_psych, og_image, ...). See Models/Enums.cs for the full mapping notes.
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
});

// --- Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:Default")));

// --- JWT auth
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing Jwt configuration section");
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("EditorOrOwner", p => p.RequireRole("Editor", "Owner"))
    .AddPolicy("OwnerOnly", p => p.RequireRole("Owner"));

// --- CORS. AllowCredentials + explicit origins (not AllowAnyOrigin — required
// for the httpOnly refresh cookie to be sent cross-origin). In production the
// admin panel and this API are same-origin behind Caddy on the droplet (per
// CLAUDE.md's hosting split), so this mainly matters for local dev where the
// Vite dev server and this API run on different ports — see the /api proxy
// added to apps/admin/vite.config.ts, which makes even local dev same-origin
// from the browser's point of view and avoids needing this at all in practice.
// Kept configurable via appsettings for whichever origins actually need it.
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

// --- Email (Resend). Logs instead of sending when no API key is configured
// (the default in dev) — see Services/EmailSender.cs.
var resendOptions = builder.Configuration.GetSection(ResendOptions.SectionName).Get<ResendOptions>()
    ?? new ResendOptions();
builder.Services.AddSingleton(resendOptions);
if (!string.IsNullOrWhiteSpace(resendOptions.ApiKey))
{
    builder.Services.AddResend(options => options.ApiToken = resendOptions.ApiKey);
    builder.Services.AddSingleton<IEmailSender, ResendEmailSender>();
}
else
{
    builder.Services.AddSingleton<IEmailSender, LoggingEmailSender>();
}

// --- Rate limiting. Only applied to the public, unauthenticated application
// submission endpoint (see ApplicationEndpoints.cs) — 5 requests/hour/IP.
builder.Services.AddRateLimiter(options =>
{
    // Default rejection status is 503 — 429 is the semantically correct one
    // for "you've been throttled" vs. "the service is down".
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("ApplicationSubmit", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromHours(1),
                QueueLimit = 0,
            }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme.",
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = [],
    });
});

var app = builder.Build();

// Serves uploaded media from wwwroot/uploads (see MediaEndpoints.cs). Uses an
// explicit PhysicalFileProvider scoped to just this folder rather than the
// bare UseStaticFiles() default (env.WebRootFileProvider): that default is
// frozen to a NullFileProvider for the app's whole lifetime if wwwroot doesn't
// exist at startup, so files written later would 404 forever even after the
// folder exists — this repo has no wwwroot until the directory is created
// here. No auth gate — the public blog also renders these via
// PublicPostDto.FeaturedImageUrl, so they need to be reachable unauthenticated,
// same as any other static asset.
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Dev convenience only — applies pending EF Core migrations and seeds the
    // three demo accounts on startup. Don't do this in production; run
    // `dotnet ef database update` as an explicit deploy step instead.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapAuthEndpoints();
app.MapPostEndpoints();
app.MapMediaEndpoints();
app.MapCategoryEndpoints();
app.MapApplicationEndpoints();
app.MapUserEndpoints();
app.MapSettingsEndpoints();
app.MapActivityEndpoints();
app.MapPublicEndpoints();

app.Run();

public partial class Program { }
