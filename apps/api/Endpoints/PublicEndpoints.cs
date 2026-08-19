using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class PublicEndpoints
{
    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public").WithTags("Public").AllowAnonymous();

        group.MapGet("/posts", async (string? category, string? tag, AppDbContext db) =>
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
            var query = db.Posts.Include(p => p.Author).Include(p => p.FeaturedImage).Include(p => p.Category)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .Where(p => p.Status == PostStatus.Published && p.PublishedAt != null);

            if (category is not null) query = query.Where(p => p.Category!.Slug == category);
            if (tag is not null) query = query.Where(p => p.PostTags.Any(pt => pt.Tag!.Slug == tag));

            var posts = await query.OrderByDescending(p => p.PublishedAt).ToListAsync();
            var dispatchNumbers = await BuildDispatchNumbersAsync(db);

            return Results.Ok(posts.Select(p => p.ToPublicDto(dispatchNumbers[p.Id])));
        });

        group.MapGet("/posts/{slug}", async (string slug, AppDbContext db) =>
        {
            var post = await db.Posts.Include(p => p.Author).Include(p => p.FeaturedImage).Include(p => p.Category)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == PostStatus.Published && p.PublishedAt != null);
            if (post is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            var dispatchNumbers = await BuildDispatchNumbersAsync(db);
            return Results.Ok(post.ToPublicDto(dispatchNumbers[post.Id]));
        });

        // Ordered by Position — the blog renders columns in this order and
        // paginates through them 3 at a time in this order.
        group.MapGet("/categories", async (AppDbContext db) =>
        {
            var categories = await db.Categories
                .Where(c => c.IsVisible && !c.IsDeleted)
                .OrderBy(c => c.Position).ThenBy(c => c.CreatedAt)
                .ToListAsync();

            var postCounts = await db.Posts
                .Where(p => p.Status == PostStatus.Published)
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            return Results.Ok(categories.Select(c => c.ToPublicDto(postCounts.GetValueOrDefault(c.Id))));
        });

        group.MapGet("/tags", async (AppDbContext db) =>
        {
            var tags = await db.Tags.OrderBy(t => t.Name).ToListAsync();
            var postCounts = await db.PostTags
                .Where(pt => pt.Post!.Status == PostStatus.Published)
                .GroupBy(pt => pt.TagId)
                .Select(g => new { TagId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TagId, x => x.Count);

            return Results.Ok(tags.Select(t => t.ToPublicDto(postCounts.GetValueOrDefault(t.Id))));
        });

        group.MapGet("/authors/{handle}", async (string handle, AppDbContext db) =>
        {
            var author = await db.Users.FirstOrDefaultAsync(u => u.Handle == handle);
            if (author is null) return Results.Json(new { message = "Not found" }, statusCode: 404);
            return Results.Ok(author.ToPublicDto());
        });

        // Sidebar turntable playlist — scans wwwroot/audio (bind-mounted to a
        // plain host folder in prod, see docker-compose.prod.yml) rather than
        // reading from a database. Tracks are added by scp'ing a file onto the
        // droplet and naming it "Artist - Title.ext", nothing more — see
        // AudioTrackScanner for the parsing rules. Absolute URLs are built from
        // ApiPublicOrigin, not the incoming request's Host, for the same reason
        // MediaEndpoints.cs does: the blog calls this cross-origin, and the one
        // origin that actually serves /audio/* is this API's own public origin
        // regardless of which Host header a given request arrived with.
        group.MapGet("/audio", (IWebHostEnvironment env, IConfiguration configuration) =>
        {
            var audioDir = Path.Combine(env.ContentRootPath, "wwwroot", "audio");
            var origin = configuration["ApiPublicOrigin"];
            var tracks = AudioTrackScanner.Scan(audioDir)
                .Select(t => t with { Src = $"{origin}{t.Src}" });
            return Results.Ok(tracks);
        });
    }

    // Dispatch number = a post's 1-based rank by PublishedAt ascending
    // (oldest = 1) among Published posts sharing its Category. Computed for
    // every published post at once, not per-returned-post, since each
    // number depends on the full ordering within its category.
    private static async Task<Dictionary<Guid, int>> BuildDispatchNumbersAsync(AppDbContext db)
    {
        var published = await db.Posts
            .Where(p => p.Status == PostStatus.Published && p.PublishedAt != null)
            .OrderBy(p => p.PublishedAt)
            .Select(p => new { p.Id, p.CategoryId })
            .ToListAsync();

        var numbers = new Dictionary<Guid, int>();
        var countByCategory = new Dictionary<Guid, int>();
        foreach (var post in published)
        {
            countByCategory[post.CategoryId] = countByCategory.GetValueOrDefault(post.CategoryId) + 1;
            numbers[post.Id] = countByCategory[post.CategoryId];
        }
        return numbers;
    }
}
