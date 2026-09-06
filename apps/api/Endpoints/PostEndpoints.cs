using System.Security.Claims;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts").WithTags("Posts").RequireAuthorization();

        group.MapGet("/", async (bool? mine, ClaimsPrincipal user, AppDbContext db) =>
        {
            var query = db.Posts.Include(p => p.Author).Include(p => p.Category).Include(p => p.PostTags).ThenInclude(pt => pt.Tag).Include(p => p.ReviewNotes).ThenInclude(r => r.Reviewer).AsQueryable();

            if (mine == true)
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("sub")!);
                query = query.Where(p => p.AuthorId == userId);
            }

            var posts = await query.OrderByDescending(p => p.UpdatedAt).ToListAsync();
            return Results.Ok(posts.Select(p => p.ToDto()));
        });

        group.MapPost("/", async (CreatePostRequest request, ClaimsPrincipal user, AppDbContext db) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")!);

            if (request.CategoryId is not null && !await db.Categories.AnyAsync(c => c.Id == request.CategoryId.Value && !c.IsDeleted))
            {
                return Results.Json(new { message = "The specified category does not exist." }, statusCode: 400);
            }

            List<Tag> tags = [];
            if (request.TagIds is { Count: > 0 })
            {
                tags = await db.Tags.Where(t => request.TagIds.Contains(t.Id)).ToListAsync();
                var missingTagIds = request.TagIds.Except(tags.Select(t => t.Id)).ToList();
                if (missingTagIds.Count > 0)
                {
                    return Results.Json(new { message = $"Unknown tag id(s): {string.Join(", ", missingTagIds)}" }, statusCode: 400);
                }
            }

            var title = request.Title ?? "Untitled draft";
            var slug = await SlugGenerator.GenerateUniqueAsync(title, s => db.Posts.AnyAsync(p => p.Slug == s));

            Guid categoryId;
            if (request.CategoryId is { } requestedCategoryId)
            {
                categoryId = requestedCategoryId;
            }
            else
            {
                var defaultCategoryId = await db.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Position).Select(c => c.Id).FirstOrDefaultAsync();
                if (defaultCategoryId == Guid.Empty)
                {
                    return Results.Json(new { message = "No category exists — create one first." }, statusCode: 400);
                }
                categoryId = defaultCategoryId;
            }

            var status = request.Status ?? PostStatus.Draft;
            var post = new Post
            {
                Title = title,
                Slug = slug,
                BodyHtml = request.BodyHtml ?? "",
                Excerpt = request.Excerpt ?? "",
                SeoTitle = request.SeoTitle ?? "",
                MetaDescription = request.MetaDescription ?? "",
                Source = request.Source,
                FeaturedImageId = request.FeaturedImageId,
                CategoryId = categoryId,
                Format = request.Format ?? PostFormat.Shelf,
                Status = status,
                PublishedAt = status == PostStatus.Published ? DateTimeOffset.UtcNow : null,
                AuthorId = userId,
            };

            db.Posts.Add(post);
            await db.SaveChangesAsync();

            if (tags.Count > 0)
            {
                db.PostTags.AddRange(tags.Select(t => new PostTag { PostId = post.Id, TagId = t.Id }));
                await db.SaveChangesAsync();
            }

            return Results.Created($"/api/posts/{post.Id}", post.ToDto());
        });

        group.MapPatch("/{id:guid}", async (Guid id, UpdatePostRequest request, AppDbContext db, ClaimsPrincipal user) =>
        {
            var post = await db.Posts.Include(p => p.Author).Include(p => p.Category).Include(p => p.PostTags).ThenInclude(pt => pt.Tag).Include(p => p.ReviewNotes).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);
            if (post.AuthorId != userId)
            {
                return Results.Json(new { message = "You can only edit your own posts." }, statusCode: 403);
            }

            var canPublishDirectly = user.IsInRole("Editor") || user.IsInRole("Owner");
            if (request.Status == PostStatus.ChangesRequested
                || (request.Status == PostStatus.Published && !canPublishDirectly))
            {
                return Results.Json(
                    new { message = "Use the approve or request-changes actions to change a post's review status." },
                    statusCode: 400);
            }

            if (request.Title is not null)
            {
                post.Title = request.Title;
                if (post.PublishedAt is null)
                {
                    post.Slug = await SlugGenerator.GenerateUniqueAsync(
                        request.Title, s => db.Posts.AnyAsync(p => p.Slug == s && p.Id != post.Id));
                }
            }
            if (request.BodyHtml is not null) post.BodyHtml = request.BodyHtml;
            if (request.Excerpt is not null) post.Excerpt = request.Excerpt;
            if (request.SeoTitle is not null) post.SeoTitle = request.SeoTitle;
            if (request.MetaDescription is not null) post.MetaDescription = request.MetaDescription;
            if (request.Source is not null) post.Source = request.Source;
            if (request.FeaturedImageId is not null) post.FeaturedImageId = request.FeaturedImageId;
            if (request.CategoryId is not null)
            {
                if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId.Value && !c.IsDeleted))
                {
                    return Results.Json(new { message = "The specified category does not exist." }, statusCode: 400);
                }
                post.CategoryId = request.CategoryId.Value;
            }
            if (request.TagIds is not null)
            {
                var tags = await db.Tags.Where(t => request.TagIds.Contains(t.Id)).ToListAsync();
                var missingTagIds = request.TagIds.Except(tags.Select(t => t.Id)).ToList();
                if (missingTagIds.Count > 0)
                {
                    return Results.Json(new { message = $"Unknown tag id(s): {string.Join(", ", missingTagIds)}" }, statusCode: 400);
                }
                db.PostTags.RemoveRange(post.PostTags);
                post.PostTags = tags.Select(t => new PostTag { PostId = post.Id, TagId = t.Id, Tag = t }).ToList();
            }
            if (request.Format is not null) post.Format = request.Format.Value;
            if (request.Status is not null)
            {
                post.Status = request.Status.Value;
                if (request.Status == PostStatus.Published) post.PublishedAt = DateTimeOffset.UtcNow;
            }
            post.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(post.ToDto());
        });

        group.MapPost("/{id:guid}/approve", async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var post = await db.Posts.Include(p => p.Author).Include(p => p.Category).Include(p => p.PostTags).ThenInclude(pt => pt.Tag).Include(p => p.ReviewNotes).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post is null) return Results.Json(new { message = "Not found" }, statusCode: 404);
            if (post.Status != PostStatus.PendingReview)
            {
                return Results.Json(new { message = "Only posts pending review can be approved." }, statusCode: 409);
            }

            post.Status = PostStatus.Published;
            post.PublishedAt = DateTimeOffset.UtcNow;
            post.UpdatedAt = DateTimeOffset.UtcNow;

            var approverId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);
            ActivityLogger.Log(db, approverId, $"published \"{post.Title}\"");

            await db.SaveChangesAsync();

            return Results.Ok(post.ToDto());
        })
        .RequireAuthorization("EditorOrOwner");

        group.MapPost("/{id:guid}/request-changes", async (
            Guid id, RequestChangesRequest request, AppDbContext db, ClaimsPrincipal user) =>
        {
            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return Results.Json(new { message = "A comment is required when requesting changes." }, statusCode: 400);
            }

            var post = await db.Posts.Include(p => p.Author).Include(p => p.Category).Include(p => p.PostTags).ThenInclude(pt => pt.Tag).Include(p => p.ReviewNotes).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post is null) return Results.Json(new { message = "Not found" }, statusCode: 404);
            if (post.Status != PostStatus.PendingReview)
            {
                return Results.Json(
                    new { message = "Only posts pending review can have changes requested." }, statusCode: 409);
            }

            var reviewerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);
            var reviewer = await db.Users.FindAsync(reviewerId);
            db.ReviewNotes.Add(new ReviewNote
            {
                PostId = post.Id,
                ReviewerId = reviewerId,
                Reviewer = reviewer,
                Comment = request.Comment,
            });
            post.Status = PostStatus.ChangesRequested;
            post.UpdatedAt = DateTimeOffset.UtcNow;
            ActivityLogger.Log(db, reviewerId, $"requested changes on \"{post.Title}\"");
            await db.SaveChangesAsync();

            return Results.Ok(post.ToDto());
        })
        .RequireAuthorization("EditorOrOwner");

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var post = await db.Posts.FindAsync(id);
            if (post is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);
            var isReviewer = user.IsInRole("Editor") || user.IsInRole("Owner");
            if (post.AuthorId != userId && !isReviewer)
            {
                return Results.Json(new { message = "You can only delete your own posts." }, statusCode: 403);
            }

            db.Posts.Remove(post);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
