using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class PublicEndpoints
{
    // Caps on /search input. Both are deliberately generous relative to any
    // real search a reader would type — they exist to bound the worst case
    // (one ILIKE chain per term across every published post's BodyHtml), not
    // to police normal queries.
    private const int MaxQueryLength = 100;
    private const int MaxQueryTerms = 10;

    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public").WithTags("Public").AllowAnonymous();

        group.MapGet("/posts", async (string? category, string? tag, int? limit, AppDbContext db) =>
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

            // Bounded before anything is split: this endpoint is anonymous and
            // already expensive (every term ILIKEs the full BodyHtml of every
            // published post), so a pathological query — bounded only by
            // Kestrel's 8KB request line — could otherwise chain hundreds of
            // terms × 6 ILIKE conditions into one statement. Truncating the
            // query string itself (rather than only one of the two split sites)
            // is what keeps the WHERE clause and ScoreMatch looking at the same
            // term set; the capped array is computed once here and handed to
            // ScoreMatch instead of being re-split there.
            if (query.Length > MaxQueryLength) query = query[..MaxQueryLength];

            // Matched per-word (AND across words, OR across fields per word)
            // rather than as one contiguous phrase — a query like "distributed
            // consensus" must still match "distributed consensus algorithms",
            // and a query's words don't all need to land in the same field.
            var terms = query.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Take(MaxQueryTerms)
                .ToArray();

            var candidateQuery = db.Posts
                .Include(p => p.Author).Include(p => p.FeaturedImage).Include(p => p.Category)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .Where(p => p.Status == PostStatus.Published && p.PublishedAt != null);

            foreach (var term in terms)
            {
                // ILIKE treats %, _ and \ as pattern syntax, so a literal % in a
                // user's query would otherwise match everything rather than a
                // percent sign. Escaping here (not in ScoreMatch, which uses
                // plain string.Contains and has no pattern syntax to confuse)
                // keeps the SQL side matching the literal characters typed.
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

    // Weighted, case-insensitive substring match across every searchable field
    // — a post can rank on more than one field (and more than one query word)
    // at once, so weights sum rather than short-circuiting on the first hit.
    // Scored per query word (matching the per-word WHERE clause above) rather
    // than against the query as one contiguous phrase, so a title containing
    // every word of the query still outranks a body-only match even when the
    // words aren't adjacent in the title. Takes the already-split, already-
    // capped terms rather than re-splitting the query, so scoring can never
    // consider a term the WHERE clause didn't filter on.
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

    // Backslash is Postgres's default ILIKE escape character, so escaping it
    // first (before % and _) is required — otherwise the backslashes this
    // method adds would themselves get escaped on the second pass.
    private static string EscapeLikePattern(string term) =>
        term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
