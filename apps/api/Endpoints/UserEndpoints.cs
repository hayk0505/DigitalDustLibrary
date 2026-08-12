using System.Security.Claims;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.AspNetCore.Hosting;
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

        group.MapGet("/{id:guid}/deletion-impact", async (Guid id, AppDbContext db) =>
        {
            if (!await db.Users.AnyAsync(u => u.Id == id))
                return Results.Json(new { message = "Not found" }, statusCode: 404);

            var postCount = await db.Posts.CountAsync(p => p.AuthorId == id);
            var mediaCount = await db.MediaAssets.CountAsync(m => m.UploadedById == id);
            var reviewNoteCount = await db.ReviewNotes.CountAsync(r => r.ReviewerId == id);
            // The media library is global, so another author's post may have set one of this
            // user's uploaded images as its featured image. Deleting the user hard-deletes that
            // MediaAsset (row + physical file), which silently nulls FeaturedImageId on that
            // post via the SetNull FK — this count surfaces that collateral damage up front.
            var affectedOtherPostCount = await db.Posts.CountAsync(p =>
                p.AuthorId != id && p.FeaturedImageId != null &&
                db.MediaAssets.Any(m => m.Id == p.FeaturedImageId && m.UploadedById == id));
            return Results.Ok(new UserDeletionImpactDto(postCount, mediaCount, reviewNoteCount, affectedOtherPostCount));
        });

        // DELETE — permanently removes the account plus their posts, uploaded
        // media (DB row + physical file), and review notes they wrote on other
        // authors' posts. Activity log entries where they were the actor are
        // preserved with ActorId set to null (see AppDbContext's SetNull config)
        // rather than deleted — the log stays an audit trail even after the
        // actor is gone.
        group.MapDelete("/{id:guid}", async (
            Guid id, AppDbContext db, IWebHostEnvironment env, ClaimsPrincipal caller, ILogger<Program> logger) =>
        {
            var target = await db.Users.FindAsync(id);
            if (target is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            var callerId = Guid.Parse(caller.FindFirstValue(ClaimTypes.NameIdentifier) ?? caller.FindFirstValue("sub")!);
            if (target.Id == callerId)
            {
                return Results.Json(new { message = "You can't delete your own account." }, statusCode: 409);
            }

            var posts = await db.Posts.Where(p => p.AuthorId == id).ToListAsync();
            db.Posts.RemoveRange(posts);

            var reviewNotesAsReviewer = await db.ReviewNotes.Where(r => r.ReviewerId == id).ToListAsync();
            db.ReviewNotes.RemoveRange(reviewNotesAsReviewer);

            var media = await db.MediaAssets.Where(m => m.UploadedById == id).ToListAsync();
            db.MediaAssets.RemoveRange(media);

            ActivityLogger.Log(db, callerId, $"deleted {target.Name}'s account");

            db.Users.Remove(target);

            await db.SaveChangesAsync();

            // DB state is now fully committed and consistent. Physical file cleanup is
            // best-effort from here on — a failure here means an orphaned file on disk,
            // not a data-integrity problem, since every MediaAsset row is already gone.
            var uploadsDir = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
            foreach (var asset in media)
            {
                try
                {
                    var filePath = Path.Combine(uploadsDir, Path.GetFileName(new Uri(asset.Url).AbsolutePath));
                    if (File.Exists(filePath)) File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to delete media file for asset {AssetId} (user {UserId})", asset.Id, id);
                }
            }

            return Results.NoContent();
        });
    }
}
