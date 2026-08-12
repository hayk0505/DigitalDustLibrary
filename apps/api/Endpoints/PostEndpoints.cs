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

        // GET /api/posts?mine=true — matches mocks/handlers/posts.ts.
        group.MapGet("/", async (bool? mine, ClaimsPrincipal user, AppDbContext db) =>
        {
            var query = db.Posts.Include(p => p.Author).Include(p => p.Category).Include(p => p.ReviewNotes).ThenInclude(r => r.Reviewer).AsQueryable();

            if (mine == true)
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("sub")!);
                query = query.Where(p => p.AuthorId == userId);
            }

            var posts = await query.OrderByDescending(p => p.UpdatedAt).ToListAsync();
            return Results.Ok(posts.Select(p => p.ToDto()));
        });

        // POST /api/posts — same defaults as the mock (Untitled draft / tech / draft)
        // when fields are omitted.
        group.MapPost("/", async (CreatePostRequest request, ClaimsPrincipal user, AppDbContext db) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")!);

            // !IsDeleted filter: a soft-deleted category is "gone" from the
            // client's perspective (it's excluded from the public blog and
            // the post editor's picker) even though its row still exists for
            // hard-delete-blocking purposes — treat an explicitly-supplied
            // soft-deleted category ID the same as a nonexistent one. Does
            // NOT filter on IsVisible: a hidden-but-not-deleted category is
            // still a legitimate assignment target (e.g. staging drafts in a
            // category ahead of its public launch).
            if (request.CategoryId is not null && !await db.Categories.AnyAsync(c => c.Id == request.CategoryId.Value && !c.IsDeleted))
            {
                return Results.Json(new { message = "The specified category does not exist." }, statusCode: 400);
            }

            var title = request.Title ?? "Untitled draft";
            var slug = await SlugGenerator.GenerateUniqueAsync(title, s => db.Posts.AnyAsync(p => p.Slug == s));

            // Defaults to the lowest-Position category when omitted, the
            // same "first in display order" rule Categories themselves use
            // for their own default ordering — not hardcoded to a specific
            // category, since categories are no longer a fixed set of 3.
            // FirstOrDefaultAsync, not FirstAsync: every category can in
            // principle be hard-deleted (nothing stops it once no post
            // references any of them), and an empty Categories table must
            // produce a clean 400 here, not an unhandled
            // InvalidOperationException -> bare 500.
            Guid categoryId;
            if (request.CategoryId is { } requestedCategoryId)
            {
                categoryId = requestedCategoryId;
            }
            else
            {
                // !IsDeleted filter: a soft-deleted category must never be
                // picked as the silent default for an omitted CategoryId —
                // it's invisible on the public blog, so a post landing there
                // by default would be unfindable. IsVisible is deliberately
                // NOT filtered here for the same reason as above: hidden
                // categories are still valid targets, including as a default.
                var defaultCategoryId = await db.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Position).Select(c => c.Id).FirstOrDefaultAsync();
                if (defaultCategoryId == Guid.Empty)
                {
                    return Results.Json(new { message = "No category exists — create one first." }, statusCode: 400);
                }
                categoryId = defaultCategoryId;
            }

            var post = new Post
            {
                Title = title,
                Slug = slug,
                BodyHtml = request.BodyHtml ?? "",
                Excerpt = request.Excerpt ?? "",
                SeoTitle = request.SeoTitle ?? "",
                MetaDescription = request.MetaDescription ?? "",
                FeaturedImageId = request.FeaturedImageId,
                CategoryId = categoryId,
                Status = request.Status ?? PostStatus.Draft,
                AuthorId = userId,
            };

            db.Posts.Add(post);
            await db.SaveChangesAsync();
            return Results.Created($"/api/posts/{post.Id}", post.ToDto());
        });

        // PATCH /api/posts/:id — partial update, only touches fields present in
        // the request. Author-only (own posts): edits content and can move
        // Draft <-> PendingReview (Save Draft / Submit for Review). Published
        // and ChangesRequested are reachable only via /approve and
        // /request-changes below — otherwise an author could self-publish by
        // PATCHing status directly, bypassing review entirely.
        group.MapPatch("/{id:guid}", async (Guid id, UpdatePostRequest request, AppDbContext db, ClaimsPrincipal user) =>
        {
            var post = await db.Posts.Include(p => p.Author).Include(p => p.Category).Include(p => p.ReviewNotes).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);
            if (post.AuthorId != userId)
            {
                return Results.Json(new { message = "You can only edit your own posts." }, statusCode: 403);
            }

            // Editor/Owner authors can publish their own post directly — they
            // already have approve authority over everyone else's, so routing
            // their own through Pending Review first is redundant. Authors
            // (no review authority) still can't; ChangesRequested is never
            // valid here regardless of role — an author can't meaningfully
            // request changes from themselves, and that transition needs a
            // review comment, which self-PATCH has no place for.
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
            if (request.FeaturedImageId is not null) post.FeaturedImageId = request.FeaturedImageId;
            if (request.CategoryId is not null)
            {
                // !IsDeleted filter — same rationale as the POST /api/posts
                // validation above: a soft-deleted category is not a valid
                // assignment target, but IsVisible is not filtered (hidden
                // categories remain assignable).
                if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId.Value && !c.IsDeleted))
                {
                    return Results.Json(new { message = "The specified category does not exist." }, statusCode: 400);
                }
                post.CategoryId = request.CategoryId.Value;
            }
            if (request.Status is not null)
            {
                post.Status = request.Status.Value;
                if (request.Status == PostStatus.Published) post.PublishedAt = DateTimeOffset.UtcNow;
            }
            post.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(post.ToDto());
        });

        // POST /api/posts/{id}/approve
        group.MapPost("/{id:guid}/approve", async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var post = await db.Posts.Include(p => p.Author).Include(p => p.Category).Include(p => p.ReviewNotes).ThenInclude(r => r.Reviewer)
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

        // POST /api/posts/{id}/request-changes
        group.MapPost("/{id:guid}/request-changes", async (
            Guid id, RequestChangesRequest request, AppDbContext db, ClaimsPrincipal user) =>
        {
            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                return Results.Json(new { message = "A comment is required when requesting changes." }, statusCode: 400);
            }

            var post = await db.Posts.Include(p => p.Author).Include(p => p.Category).Include(p => p.ReviewNotes).ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (post is null) return Results.Json(new { message = "Not found" }, statusCode: 404);
            if (post.Status != PostStatus.PendingReview)
            {
                return Results.Json(
                    new { message = "Only posts pending review can have changes requested." }, statusCode: 409);
            }

            var reviewerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);
            var reviewer = await db.Users.FindAsync(reviewerId);
            // Added via db.ReviewNotes, not post.ReviewNotes.Add(...) — EF Core
            // misreads a client-key-generated entity discovered only through
            // navigation-collection fixup as an existing row to UPDATE rather
            // than a new one to INSERT, throwing a spurious
            // DbUpdateConcurrencyException ("0 rows affected"). Every other
            // entity in this codebase is added straight to its DbSet for
            // exactly this reason — this is the first place that wasn't.
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
    }
}
