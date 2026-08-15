using System.Security.Claims;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        // Group requires only an authenticated user — GET is readable by any
        // role (Authors need the list to populate the post editor's category
        // picker, mirroring PostEndpoints.cs's group-level bare
        // RequireAuthorization()). The write routes (POST/PATCH/DELETE) chain
        // their own stricter "EditorOrOwner" policy individually below, since
        // Authors still shouldn't be able to create/edit/delete categories.
        var group = app.MapGroup("/api/categories").WithTags("Categories").RequireAuthorization();

        // GET /api/categories — returns everything, including hidden and
        // soft-deleted categories. This is the admin management list, not the
        // public blog's category filter (that's a different, not-yet-built
        // public endpoint that WOULD filter to IsVisible && !IsDeleted only).
        // Ordered by Position (admin-controlled display/paging order), then
        // CreatedAt as a tie-breaker — Position isn't unique at the schema
        // level (drag-and-drop reordering writes plain ints, ties are
        // possible), so the tie-breaker keeps this deterministic.
        group.MapGet("/", async (AppDbContext db) =>
        {
            var categories = await db.Categories.OrderBy(c => c.Position).ThenBy(c => c.CreatedAt).ToListAsync();
            var postCounts = await db.Posts
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            return Results.Ok(categories.Select(c => c.ToDto(postCounts.GetValueOrDefault(c.Id))));
        });

        group.MapPost("/", async (CreateCategoryRequest request, AppDbContext db) =>
        {
            if (await db.Categories.AnyAsync(c => c.Slug == request.Slug))
            {
                return Results.Json(new { message = $"Slug '{request.Slug}' is already in use." }, statusCode: 409);
            }

            // Position defaults to "append to the end" when omitted — max()
            // over zero existing categories is null, not 0, so that case is
            // handled explicitly rather than letting the very first category
            // ever created end up with a null Position.
            var position = request.Position
                ?? (await db.Categories.MaxAsync(c => (int?)c.Position) ?? 0) + 1;

            var category = new Category
            {
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                Color = request.Color,
                FolderColor = request.FolderColor,
                Position = position,
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return Results.Created($"/api/categories/{category.Id}", category.ToDto(0));
        })
        .RequireAuthorization("EditorOrOwner");

        // PATCH — covers hide/show (IsVisible), soft-delete/restore (IsDeleted),
        // and renaming, all through the same partial-update shape as posts.
        group.MapPatch("/{id:guid}", async (Guid id, UpdateCategoryRequest request, AppDbContext db, ClaimsPrincipal user) =>
        {
            var category = await db.Categories.FindAsync(id);
            if (category is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            if (request.Slug is not null && request.Slug != category.Slug
                && await db.Categories.AnyAsync(c => c.Slug == request.Slug && c.Id != id))
            {
                return Results.Json(new { message = $"Slug '{request.Slug}' is already in use." }, statusCode: 409);
            }

            var actorId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);

            if (request.Name is not null) category.Name = request.Name;
            if (request.Slug is not null) category.Slug = request.Slug;
            if (request.Description is not null) category.Description = request.Description;
            if (request.Color is not null) category.Color = request.Color;
            if (request.FolderColor is not null) category.FolderColor = request.FolderColor;
            if (request.Position is not null) category.Position = request.Position.Value;
            if (request.IsVisible is not null && request.IsVisible.Value != category.IsVisible)
            {
                category.IsVisible = request.IsVisible.Value;
                ActivityLogger.Log(db, actorId, category.IsVisible ? $"unhid \"{category.Name}\"" : $"hid \"{category.Name}\"");
            }
            if (request.IsDeleted is not null && request.IsDeleted.Value != category.IsDeleted)
            {
                category.IsDeleted = request.IsDeleted.Value;
                category.DeletedAt = request.IsDeleted.Value ? DateTimeOffset.UtcNow : null;
                ActivityLogger.Log(db, actorId, category.IsDeleted ? $"archived \"{category.Name}\"" : $"restored \"{category.Name}\"");
            }

            await db.SaveChangesAsync();
            var postCount = await db.Posts.CountAsync(p => p.CategoryId == id);
            return Results.Ok(category.ToDto(postCount));
        })
        .RequireAuthorization("EditorOrOwner");

        // DELETE — true hard delete. Blocked whenever any post (including
        // already-published ones) still references this category, per the
        // "hide/soft-delete vs blocked hard-delete" rule in
        // Functional_Overview_for_Design.md. Use PATCH { isDeleted: true }
        // (soft delete) instead if posts still reference it.
        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var category = await db.Categories.FindAsync(id);
            if (category is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            var postCount = await db.Posts.CountAsync(p => p.CategoryId == id);
            if (postCount > 0)
            {
                return Results.Json(new
                {
                    message = $"Cannot delete '{category.Name}' — {postCount} post(s) still reference it. "
                        + "Reassign those posts first, or use soft-delete (PATCH isDeleted: true) instead.",
                }, statusCode: 409);
            }

            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .RequireAuthorization("EditorOrOwner");
    }
}
