using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class TagEndpoints
{
    public static void MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        // Bare RequireAuthorization() at the group level, same as
        // CategoryEndpoints — any authenticated role can read. Unlike
        // Category, POST also stays at this bare level (see Task 2): any
        // role can create a tag, since Authors free-type them while
        // drafting. Only PATCH/merge/DELETE (Tasks 3-4) chain the stricter
        // "EditorOrOwner" policy.
        var group = app.MapGroup("/api/tags").WithTags("Tags").RequireAuthorization();

        group.MapGet("/", async (AppDbContext db) =>
        {
            var tags = await db.Tags.OrderBy(t => t.Name).ToListAsync();
            var postCounts = await db.PostTags
                .GroupBy(pt => pt.TagId)
                .Select(g => new { TagId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TagId, x => x.Count);

            return Results.Ok(tags.Select(t => t.ToDto(postCounts.GetValueOrDefault(t.Id))));
        });

        // POST — get-or-create semantics (full behavior specified for Task 2
        // in docs/superpowers/specs/2026-08-17-tags-design.md, pulled forward
        // minimally here because this task's own TagListTests.
        // Get_ReturnsTagsOrderedByName, via TagTestHelpers.CreateTagAsync,
        // exercises POST /api/tags — see task-1-report.md for why). Slug is
        // computed via the plain, non-uniquified SlugGenerator.Slugify (not
        // GenerateUniqueAsync): if a Tag with that exact slug already exists,
        // it's returned as-is (200) instead of minting a near-duplicate, so
        // two authors (or the same author twice) free-typing "AI" both
        // resolve to the same tag.
        group.MapPost("/", async (CreateTagRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.Json(new { message = "Name is required." }, statusCode: 400);
            }

            var slug = SlugGenerator.Slugify(request.Name);

            var existing = await db.Tags.FirstOrDefaultAsync(t => t.Slug == slug);
            if (existing is not null)
            {
                var existingPostCount = await db.PostTags.CountAsync(pt => pt.TagId == existing.Id);
                return Results.Ok(existing.ToDto(existingPostCount));
            }

            var tag = new Tag { Name = request.Name, Slug = slug };
            db.Tags.Add(tag);
            await db.SaveChangesAsync();
            return Results.Created($"/api/tags/{tag.Id}", tag.ToDto(0));
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdateTagRequest request, AppDbContext db) =>
        {
            var tag = await db.Tags.FindAsync(id);
            if (tag is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            if (request.Slug is not null && request.Slug != tag.Slug
                && await db.Tags.AnyAsync(t => t.Slug == request.Slug && t.Id != id))
            {
                return Results.Json(new { message = $"Slug '{request.Slug}' is already in use." }, statusCode: 409);
            }

            if (request.Name is not null) tag.Name = request.Name;
            if (request.Slug is not null) tag.Slug = request.Slug;

            await db.SaveChangesAsync();
            var postCount = await db.PostTags.CountAsync(pt => pt.TagId == id);
            return Results.Ok(tag.ToDto(postCount));
        })
        .RequireAuthorization("EditorOrOwner");

        // Hard delete, never blocked by post count (unlike Category) —
        // untagging isn't destructive to a post the way losing its only
        // category would be. PostTags cascade-deletes at the DB level (see
        // AppDbContext's OnModelCreating).
        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var tag = await db.Tags.FindAsync(id);
            if (tag is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            db.Tags.Remove(tag);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .RequireAuthorization("EditorOrOwner");

        group.MapPost("/{id:guid}/merge", async (Guid id, MergeTagRequest request, AppDbContext db) =>
        {
            if (id == request.TargetTagId)
            {
                return Results.Json(new { message = "Cannot merge a tag into itself." }, statusCode: 400);
            }

            var source = await db.Tags.FindAsync(id);
            if (source is null) return Results.Json(new { message = "Not found" }, statusCode: 404);
            var target = await db.Tags.FindAsync(request.TargetTagId);
            if (target is null) return Results.Json(new { message = "Target tag not found." }, statusCode: 404);

            var sourceLinks = await db.PostTags.Where(pt => pt.TagId == id).ToListAsync();
            var targetPostIds = (await db.PostTags.Where(pt => pt.TagId == request.TargetTagId).Select(pt => pt.PostId).ToListAsync())
                .ToHashSet();

            // Remove + (conditionally) re-add rather than mutating link.TagId
            // in place — TagId is half of PostTag's composite primary key,
            // and this keeps the change tracker's job unambiguous: every
            // source link is definitely gone, and a post that already had
            // the target tag doesn't get a duplicate row (which would
            // otherwise violate the (PostId, TagId) primary key).
            foreach (var link in sourceLinks)
            {
                db.PostTags.Remove(link);
                if (!targetPostIds.Contains(link.PostId))
                {
                    db.PostTags.Add(new PostTag { PostId = link.PostId, TagId = target.Id });
                }
            }

            db.Tags.Remove(source);
            await db.SaveChangesAsync();

            var postCount = await db.PostTags.CountAsync(pt => pt.TagId == target.Id);
            return Results.Ok(target.ToDto(postCount));
        })
        .RequireAuthorization("EditorOrOwner");
    }
}
