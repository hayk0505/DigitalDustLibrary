using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class PublicEndpoints
{
    private const int MaxQueryLength = 100;
    private const int MaxQueryTerms = 10;

    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public").WithTags("Public").AllowAnonymous();

        group.MapGet("/posts", async (string? category, string? tag, string? format, int? limit, AppDbContext db) =>
        {
            var query = db.Posts.Include(p => p.Author).Include(p => p.FeaturedImage).Include(p => p.Category)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .Where(p => p.Status == PostStatus.Published && p.PublishedAt != null);

            if (category is not null) query = query.Where(p => p.Category!.Slug == category);
            if (tag is not null) query = query.Where(p => p.PostTags.Any(pt => pt.Tag!.Slug == tag));
            if (format is not null && Enum.TryParse<PostFormat>(format, ignoreCase: true, out var parsedFormat))
            {
                query = query.Where(p => p.Format == parsedFormat);
            }

            var ordered = query.OrderByDescending(p => p.PublishedAt);
            var posts = await (limit.HasValue && limit.Value > 0 ? ordered.Take(limit.Value) : ordered).ToListAsync();
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

        group.MapGet("/search", async (string? q, AppDbContext db) =>
        {
            var query = (q ?? "").Trim();
            if (query.Length < 2) return Results.Ok(Array.Empty<PublicPostDto>());

            if (query.Length > MaxQueryLength) query = query[..MaxQueryLength];

            var terms = query.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Take(MaxQueryTerms)
                .ToArray();

            var candidateQuery = db.Posts
                .Include(p => p.Author).Include(p => p.FeaturedImage).Include(p => p.Category)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .Where(p => p.Status == PostStatus.Published && p.PublishedAt != null);

            foreach (var term in terms)
            {
                var pattern = $"%{EscapeLikePattern(term)}%";
                candidateQuery = candidateQuery.Where(p =>
                    EF.Functions.ILike(p.Title, pattern) ||
                    EF.Functions.ILike(p.Excerpt, pattern) ||
                    EF.Functions.ILike(p.BodyHtml, pattern) ||
                    EF.Functions.ILike(p.Category!.Name, pattern) ||
                    EF.Functions.ILike(p.Author!.Name, pattern) ||
                    p.PostTags.Any(pt => EF.Functions.ILike(pt.Tag!.Name, pattern)));
            }

            var posts = await candidateQuery.ToListAsync();

            var dispatchNumbers = await BuildDispatchNumbersAsync(db);

            var ranked = posts
                .Select(p => (Post: p, Score: ScoreMatch(p, terms)))
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Post.PublishedAt)
                .Take(50)
                .Select(x => x.Post.ToPublicDto(dispatchNumbers[x.Post.Id]));

            return Results.Ok(ranked);
        });

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

        group.MapGet("/audio", (IWebHostEnvironment env, IConfiguration configuration) =>
        {
            var audioDir = Path.Combine(env.ContentRootPath, "wwwroot", "audio");
            var origin = configuration["ApiPublicOrigin"];
            var tracks = AudioTrackScanner.Scan(audioDir)
                .Select(t => t with { Src = $"{origin}{t.Src}" });
            return Results.Ok(tracks);
        });

        group.MapGet("/settings", async (AppDbContext db) =>
        {
            var settings = await db.SiteSettings.SingleAsync();
            return Results.Ok(settings.ToDto());
        });
    }

    private static async Task<Dictionary<Guid, int>> BuildDispatchNumbersAsync(AppDbContext db)
    {
        var published = await db.Posts
            .Where(p => p.Status == PostStatus.Published && p.PublishedAt != null)
            .OrderBy(p => p.PublishedAt)
            .Select(p => new { p.Id, p.CategoryId, p.Format })
            .ToListAsync();

        var numbers = new Dictionary<Guid, int>();
        var countByGroup = new Dictionary<(Guid CategoryId, PostFormat Format), int>();
        foreach (var post in published)
        {
            var key = (post.CategoryId, post.Format);
            countByGroup[key] = countByGroup.GetValueOrDefault(key) + 1;
            numbers[post.Id] = countByGroup[key];
        }
        return numbers;
    }

    private static int ScoreMatch(Post p, string[] terms)
    {
        var score = 0;
        foreach (var term in terms)
        {
            if (Contains(p.Title, term)) score += 50;
            if (Contains(p.Category?.Name, term)) score += 30;
            if (p.PostTags.Any(pt => Contains(pt.Tag?.Name, term))) score += 30;
            if (Contains(p.Excerpt, term)) score += 20;
            if (Contains(p.Author?.Name, term)) score += 15;
            if (Contains(p.BodyHtml, term)) score += 5;
        }
        return score;
    }

    private static bool Contains(string? haystack, string needle) =>
        haystack is not null && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);

    private static string EscapeLikePattern(string term) =>
        term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
