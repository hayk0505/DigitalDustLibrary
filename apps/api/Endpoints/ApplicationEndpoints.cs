using System.Security.Claims;
using System.Security.Cryptography;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class ApplicationEndpoints
{
    private const int InviteExpiryDays = 7;

    public static void MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/applications").WithTags("Applications");

        // POST /api/applications — public, unauthenticated, rate-limited
        // (5/hour/IP, see Program.cs's "ApplicationSubmit" policy).
        group.MapPost("/", async (CreateAuthorApplicationRequest request, AppDbContext db) =>
        {
            var application = new AuthorApplication
            {
                Name = request.Name,
                Email = request.Email,
                Pitch = request.Pitch,
            };

            db.AuthorApplications.Add(application);
            await db.SaveChangesAsync();
            return Results.Created($"/api/applications/{application.Id}", application.ToDto());
        })
        .AllowAnonymous()
        .RequireRateLimiting("ApplicationSubmit");

        // GET /api/applications — returns everything regardless of status,
        // newest first; same "don't pre-filter, let the admin UI decide" choice
        // as GET /api/categories.
        group.MapGet("/", async (AppDbContext db) =>
        {
            var applications = await db.AuthorApplications
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
            return Results.Ok(applications.Select(a => a.ToDto()));
        })
        .RequireAuthorization("EditorOrOwner");

        // POST /api/applications/{id}/approve — creates the real account
        // (inactive until the invite is redeemed) and emails the invite link.
        group.MapPost("/{id:guid}/approve", async (
            Guid id, AppDbContext db, IEmailSender emailSender, IConfiguration configuration,
            ClaimsPrincipal user) =>
        {
            var application = await db.AuthorApplications.FindAsync(id);
            if (application is null) return Results.Json(new { message = "Not found" }, statusCode: 404);
            if (application.Status != ApplicationStatus.Pending)
            {
                return Results.Json(
                    new { message = "This application has already been reviewed." }, statusCode: 409);
            }

            var handle = await SlugGenerator.GenerateUniqueAsync(application.Name, h => db.Users.AnyAsync(u => u.Handle == h));
            var hasher = new PasswordHasher<ApplicationUser>();
            var newUser = new ApplicationUser
            {
                Name = application.Name,
                Handle = handle,
                Email = application.Email,
                Role = Role.Author,
                IsActive = false,
                PasswordHash = "",
            };
            // Unguessable placeholder — login is blocked anyway (IsActive: false)
            // until the invite below is redeemed and sets a real password.
            newUser.PasswordHash = hasher.HashPassword(newUser, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
            db.Users.Add(newUser);

            var rawToken = TokenService.GenerateRefreshTokenValue();
            db.InviteTokens.Add(new InviteToken
            {
                UserId = newUser.Id,
                TokenHash = TokenService.HashToken(rawToken),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(InviteExpiryDays),
            });

            var reviewerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);
            application.Status = ApplicationStatus.Approved;
            application.ReviewedAt = DateTimeOffset.UtcNow;
            application.ReviewedByUserId = reviewerId;
            ActivityLogger.Log(db, reviewerId, $"approved {application.Name}'s application");

            await db.SaveChangesAsync();

            var inviteUrl = $"{configuration["AdminFrontendUrl"]}/set-password?token={Uri.EscapeDataString(rawToken)}";
            var (subject, html) = EmailTemplates.Approved(application.Name, inviteUrl);
            await emailSender.SendAsync(application.Email, subject, html);

            return Results.Ok(application.ToDto());
        })
        .RequireAuthorization("EditorOrOwner");

        // POST /api/applications/{id}/reject
        group.MapPost("/{id:guid}/reject", async (
            Guid id, AppDbContext db, IEmailSender emailSender, ClaimsPrincipal user) =>
        {
            var application = await db.AuthorApplications.FindAsync(id);
            if (application is null) return Results.Json(new { message = "Not found" }, statusCode: 404);
            if (application.Status != ApplicationStatus.Pending)
            {
                return Results.Json(
                    new { message = "This application has already been reviewed." }, statusCode: 409);
            }

            var reviewerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);
            application.Status = ApplicationStatus.Rejected;
            application.ReviewedAt = DateTimeOffset.UtcNow;
            application.ReviewedByUserId = reviewerId;
            ActivityLogger.Log(db, reviewerId, $"rejected {application.Name}'s application");
            await db.SaveChangesAsync();

            var (subject, html) = EmailTemplates.Rejected(application.Name);
            await emailSender.SendAsync(application.Email, subject, html);

            return Results.Ok(application.ToDto());
        })
        .RequireAuthorization("EditorOrOwner");
    }
}
