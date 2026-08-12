using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class PublicEndpoints
{
    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public").WithTags("Public").AllowAnonymous();

        group.MapGet("/posts", async (Pillar? pillar, AppDbContext db) =>
        {
            // PublishedAt != null alongside the status check: the only two
            // write paths (PATCH's self-publish, /approve) always set both
            // together, but nothing at the schema level enforces that
            // invariant — a Published row somehow missing PublishedAt has
            // happened in prod before and must not 500 this list for every
            // visitor (Mapping.ToPublicDto force-unwraps PublishedAt for
            // every row here). Excluding it is correct either way: a post
            // with no publish timestamp isn't in a genuinely valid published
            // state (its date-based sort position and dispatch number would
            // be meaningless).
            var query = db.Posts.Include(p => p.Author).Include(p => p.FeaturedImage)
                .Where(p => p.Status == PostStatus.Published && p.PublishedAt != null);

            if (pillar is not null) query = query.Where(p => p.Pillar == pillar);

            var posts = await query.OrderByDescending(p => p.PublishedAt).ToListAsync();
            var dispatchNumbers = await BuildDispatchNumbersAsync(db);

            return Results.Ok(posts.Select(p => p.ToPublicDto(dispatchNumbers[p.Id])));
        });

        group.MapGet("/posts/{slug}", async (string slug, AppDbContext db) =>
        {
            var post = await db.Posts.Include(p => p.Author).Include(p => p.FeaturedImage)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == PostStatus.Published && p.PublishedAt != null);
            if (post is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            var dispatchNumbers = await BuildDispatchNumbersAsync(db);
            return Results.Ok(post.ToPublicDto(dispatchNumbers[post.Id]));
        });

        group.MapGet("/categories", async (AppDbContext db) =>
        {
            var categories = await db.Categories
                .Where(c => c.IsVisible && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return Results.Ok(categories.Select(c => c.ToPublicDto()));
        });

        group.MapGet("/authors/{handle}", async (string handle, AppDbContext db) =>
        {
            var author = await db.Users.FirstOrDefaultAsync(u => u.Handle == handle);
            if (author is null) return Results.Json(new { message = "Not found" }, statusCode: 404);
            return Results.Ok(author.ToPublicDto());
        });
    }

    // Dispatch number = a post's 1-based rank by PublishedAt ascending
    // (oldest = 1) among Published posts sharing its Pillar. Computed for
    // every published post at once, not per-returned-post, since each
    // number depends on the full ordering within its pillar.
    private static async Task<Dictionary<Guid, int>> BuildDispatchNumbersAsync(AppDbContext db)
    {
        var published = await db.Posts
            .Where(p => p.Status == PostStatus.Published && p.PublishedAt != null)
            .OrderBy(p => p.PublishedAt)
            .Select(p => new { p.Id, p.Pillar })
            .ToListAsync();

        var numbers = new Dictionary<Guid, int>();
        var countByPillar = new Dictionary<Pillar, int>();
        foreach (var post in published)
        {
            countByPillar[post.Pillar] = countByPillar.GetValueOrDefault(post.Pillar) + 1;
            numbers[post.Id] = countByPillar[post.Pillar];
        }
        return numbers;
    }
}
