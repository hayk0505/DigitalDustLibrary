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
        // Whole group requires Editor or Owner — Authors don't see this screen
        // at all per Admin_Panel_Build_Spec.md's nav table, and that has to be
        // enforced here too, not just hidden in the UI (a client-side-only
        // check is not real access control).
        var group = app.MapGroup("/api/categories").WithTags("Categories").RequireAuthorization("EditorOrOwner");

        // GET /api/categories — returns everything, including hidden and
        // soft-deleted categories. This is the admin management list, not the
        // public blog's category filter (that's a different, not-yet-built
        // public endpoint that WOULD filter to IsVisible && !IsDeleted only).
        group.MapGet("/", async (AppDbContext db) =>
        {
            var categories = await db.Categories.OrderBy(c => c.Name).ToListAsync();
            var postCounts = await db.Posts
                .Where(p => p.CategoryId != null)
                .GroupBy(p => p.CategoryId!.Value)
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

            var category = new Category
            {
                Name = request.Name,
                Slug = request.Slug,
                IsPillar = request.IsPillar ?? false,
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return Results.Created($"/api/categories/{category.Id}", category.ToDto(0));
        });

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
        });

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
        });
    }
}
