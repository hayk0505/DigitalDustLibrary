using System.Security.Claims;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization("OwnerOnly");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var users = await db.Users.OrderBy(u => u.Name).ToListAsync();
            return Results.Ok(users.Select(u => u.ToManagedDto()));
        });

        // PATCH — role change and/or deactivate, same partial-update shape as
        // Categories/Posts/Applications. Self-lockout guard: a caller can't
        // demote themselves away from Owner or deactivate themselves.
        // Deactivating revokes the target's active refresh tokens so it takes
        // effect immediately, not just on their next login attempt.
        group.MapPatch("/{id:guid}", async (Guid id, UpdateUserRequest request, AppDbContext db, ClaimsPrincipal caller) =>
        {
            var target = await db.Users.FindAsync(id);
            if (target is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            var callerId = Guid.Parse(caller.FindFirstValue(ClaimTypes.NameIdentifier) ?? caller.FindFirstValue("sub")!);
            var isSelf = target.Id == callerId;
            var wouldDemoteSelf = isSelf && request.Role is not null && request.Role != Role.Owner;
            var wouldDeactivateSelf = isSelf && request.IsActive == false;
            if (wouldDemoteSelf || wouldDeactivateSelf)
            {
                return Results.Json(
                    new { message = "You can't change your own role away from Owner or deactivate your own account." },
                    statusCode: 409);
            }

            if (request.Role is not null && request.Role.Value != target.Role)
            {
                target.Role = request.Role.Value;
                ActivityLogger.Log(db, callerId, $"changed {target.Name}'s role to {target.Role}");
            }

            if (request.IsActive is not null && request.IsActive.Value != target.IsActive)
            {
                target.IsActive = request.IsActive.Value;
                ActivityLogger.Log(db, callerId, target.IsActive ? $"reactivated {target.Name}" : $"deactivated {target.Name}");
                if (!target.IsActive)
                {
                    var activeTokens = await db.RefreshTokens
                        .Where(t => t.UserId == target.Id && t.RevokedAt == null)
                        .ToListAsync();
                    foreach (var token in activeTokens) token.RevokedAt = DateTimeOffset.UtcNow;
                }
            }

            await db.SaveChangesAsync();
            return Results.Ok(target.ToManagedDto());
        });
    }
}
