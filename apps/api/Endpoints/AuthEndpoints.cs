using System.Security.Claims;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class AuthEndpoints
{
    private const string RefreshCookieName = "refreshToken";

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        // POST /api/auth/login — matches apps/admin mocks/handlers/auth.ts exactly:
        // { email, password } -> { accessToken, user }. The mock re-derives
        // everything from the (expired) bearer token on refresh; this real
        // implementation instead uses a proper httpOnly-cookie-backed refresh
        // token, per the spec's decided auth model — same response shape either way.
        group.MapPost("/login", async (
            LoginRequest request,
            AppDbContext db,
            TokenService tokens,
            HttpResponse response,
            IWebHostEnvironment env) =>
        {
            var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
            var hasher = new PasswordHasher<ApplicationUser>();
            if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
                != PasswordVerificationResult.Success)
            {
                return Results.Json(new { message = "Invalid email or password" }, statusCode: 401);
            }

            var accessToken = tokens.CreateAccessToken(user);
            await IssueRefreshCookieAsync(db, tokens, response, env, user.Id);

            return Results.Ok(new AuthResponseDto(accessToken, user.ToDto()));
        });

        // GET /api/auth/me — "who is the current session," used by apps/admin
        // to restore auth state after a silent /refresh on page load (in-memory
        // auth state means a hard refresh has nothing else to reconstruct it from).
        group.MapGet("/me", async (ClaimsPrincipal user, AppDbContext db) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);
            var dbUser = await db.Users.FindAsync(userId);
            if (dbUser is null) return Results.Json(new { message = "Not found" }, statusCode: 404);
            return Results.Ok(dbUser.ToDto());
        })
        .RequireAuthorization();

        // POST /api/auth/accept-invite — redeems an approved application's
        // invite token (see ApplicationEndpoints' /approve route), sets the
        // real password, activates the account, and logs the user in
        // immediately (same response shape as /login).
        group.MapPost("/accept-invite", async (
            AcceptInviteRequest request, AppDbContext db, TokenService tokens,
            HttpResponse response, IWebHostEnvironment env) =>
        {
            var hash = TokenService.HashToken(request.Token);
            var stored = await db.InviteTokens.Include(i => i.User)
                .SingleOrDefaultAsync(i => i.TokenHash == hash);

            if (stored is null || !stored.IsActive || stored.User is null)
            {
                return Results.Json(new { message = "This invite link is invalid or has expired." }, statusCode: 401);
            }

            var hasher = new PasswordHasher<ApplicationUser>();
            stored.User.PasswordHash = hasher.HashPassword(stored.User, request.Password);
            stored.User.IsActive = true;
            stored.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            var accessToken = tokens.CreateAccessToken(stored.User);
            await IssueRefreshCookieAsync(db, tokens, response, env, stored.User.Id);

            return Results.Ok(new AuthResponseDto(accessToken, stored.User.ToDto()));
        })
        .AllowAnonymous();

        // POST /api/auth/refresh — reads the httpOnly cookie (not the expired
        // bearer token — that's the mock's simplification, not what the real
        // API does), rotates the refresh token, returns a new access token.
        group.MapPost("/refresh", async (
            HttpRequest request,
            HttpResponse response,
            AppDbContext db,
            TokenService tokens,
            IWebHostEnvironment env) =>
        {
            if (!request.Cookies.TryGetValue(RefreshCookieName, out var rawToken) || string.IsNullOrEmpty(rawToken))
            {
                return Results.Json(new { message = "Session expired" }, statusCode: 401);
            }

            var hash = TokenService.HashToken(rawToken);
            var stored = await db.RefreshTokens.Include(r => r.User)
                .SingleOrDefaultAsync(r => r.TokenHash == hash);

            if (stored is null || !stored.IsActive || stored.User is null || !stored.User.IsActive)
            {
                response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/auth" });
                return Results.Json(new { message = "Session expired" }, statusCode: 401);
            }

            stored.RevokedAt = DateTimeOffset.UtcNow; // rotate: old token dies here
            await IssueRefreshCookieAsync(db, tokens, response, env, stored.UserId);

            var accessToken = tokens.CreateAccessToken(stored.User);
            return Results.Ok(new { accessToken });
        });

        // POST /api/auth/logout — revokes the refresh token server-side and
        // clears the cookie. Not in the MSW mock (Phase 1 has no logout UI yet),
        // added here since a real backend needs it regardless.
        group.MapPost("/logout", async (HttpRequest request, HttpResponse response, AppDbContext db) =>
        {
            if (request.Cookies.TryGetValue(RefreshCookieName, out var rawToken) && !string.IsNullOrEmpty(rawToken))
            {
                var hash = TokenService.HashToken(rawToken);
                var stored = await db.RefreshTokens.SingleOrDefaultAsync(r => r.TokenHash == hash);
                if (stored is not null)
                {
                    stored.RevokedAt = DateTimeOffset.UtcNow;
                    await db.SaveChangesAsync();
                }
            }

            response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/auth" });
            return Results.NoContent();
        });
    }

    private static async Task IssueRefreshCookieAsync(
        AppDbContext db, TokenService tokens, HttpResponse response, IWebHostEnvironment env, Guid userId)
    {
        var raw = TokenService.GenerateRefreshTokenValue();
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = TokenService.HashToken(raw),
            ExpiresAt = tokens.RefreshTokenExpiry(),
        });
        await db.SaveChangesAsync();

        response.Cookies.Append(RefreshCookieName, raw, new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(), // plain http in local dev, https everywhere else
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = tokens.RefreshTokenExpiry(),
        });
    }
}
