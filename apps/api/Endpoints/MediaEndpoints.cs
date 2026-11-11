using System.Security.Claims;
using System.Text.RegularExpressions;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace DigitalDustLibrary.Api.Endpoints;

public static class MediaEndpoints
{
    private const long MaxUploadBytes = 8 * 1024 * 1024;

    // Uploads land in a narrow article column, never anywhere near their
    // original camera/screenshot resolution — no point storing multi-
    // thousand-px originals. Longest edge only, aspect ratio preserved;
    // never upscales (see the explicit size check before resizing below).
    private const int MaxDimension = 1200;

    private static readonly Dictionary<string, string> AllowedImageExtensions = new()
    {
        ["image/png"] = "png",
        ["image/jpeg"] = "jpg",
        ["image/webp"] = "webp",
        ["image/gif"] = "gif",
        ["image/svg+xml"] = "svg",
    };

    // SVG is a vector format with nothing to rasterize, and GIF uploads are
    // typically animated — re-encoding either as WebP would either be
    // pointless (SVG) or silently flatten the animation to one frame (GIF).
    // Both are stored byte-for-byte as uploaded instead.
    private static readonly HashSet<string> SkipTranscodeExtensions = ["svg", "gif"];

    private static readonly Regex DataUrlPattern =
        new(@"^data:(?<mime>image/[\w.+-]+);base64,(?<data>.+)$", RegexOptions.Singleline | RegexOptions.Compiled);

    public static void MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media").WithTags("Media").RequireAuthorization();

        group.MapGet("/", async (string? search, AppDbContext db) =>
        {
            var query = db.MediaAssets.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => EF.Functions.ILike(m.Filename, $"%{search}%"));
            }
            var assets = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
            return Results.Ok(assets.Select(m => m.ToDto()));
        });

        group.MapPost("/", async (
            CreateMediaRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            IWebHostEnvironment env,
            IConfiguration configuration) =>
        {
            var match = DataUrlPattern.Match(request.DataUrl);
            if (!match.Success || !AllowedImageExtensions.TryGetValue(match.Groups["mime"].Value, out var extension))
            {
                return Results.Json(new { message = "Unsupported or malformed image data" }, statusCode: 400);
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(match.Groups["data"].Value);
            }
            catch (FormatException)
            {
                return Results.Json(new { message = "Unsupported or malformed image data" }, statusCode: 400);
            }

            if (bytes.Length > MaxUploadBytes)
            {
                return Results.Json(new { message = "Image exceeds the 8 MB upload limit" }, statusCode: 400);
            }

            var width = request.Width;
            var height = request.Height;

            if (!SkipTranscodeExtensions.Contains(extension))
            {
                using var original = SKBitmap.Decode(bytes);

                // Only resize when actually over the cap — Resize() targets
                // the given box exactly, it won't skip a no-op resize for us.
                SKBitmap? downscaled = null;
                var final = original;
                if (original.Width > MaxDimension || original.Height > MaxDimension)
                {
                    var scale = (float)MaxDimension / Math.Max(original.Width, original.Height);
                    var targetSize = new SKSizeI(
                        Math.Max(1, (int)Math.Round(original.Width * scale)),
                        Math.Max(1, (int)Math.Round(original.Height * scale)));
                    downscaled = original.Resize(targetSize, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
                    final = downscaled ?? original;
                }

                using var image = SKImage.FromBitmap(final);
                using var encoded = image.Encode(SKEncodedImageFormat.Webp, 80);
                bytes = encoded.ToArray();
                extension = "webp";
                width = final.Width;
                height = final.Height;
                downscaled?.Dispose();
            }

            var uploadsDir = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");

            var storedFilename = $"{Guid.CreateVersion7()}.{extension}";
            await File.WriteAllBytesAsync(Path.Combine(uploadsDir, storedFilename), bytes);

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")!);

            var asset = new MediaAsset
            {
                Filename = request.Filename,
                Tag = request.Tag,
                Width = width,
                Height = height,
                Url = $"{configuration["ApiPublicOrigin"]}/uploads/{storedFilename}",
                UploadedById = userId,
            };

            db.MediaAssets.Add(asset);
            await db.SaveChangesAsync();
            return Results.Created($"/api/media/{asset.Id}", asset.ToDto());
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, IWebHostEnvironment env) =>
        {
            var asset = await db.MediaAssets.FindAsync(id);
            if (asset is null) return Results.Json(new { message = "Not found" }, statusCode: 404);

            var storedFilename = asset.Url.Split('/').Last();

            // A live post can use an asset two ways: as its featured image
            // (FK) or inline in the body (<img src> written by the TipTap
            // editor). The body match is on "/uploads/{file}" rather than the
            // full Url so it keeps working if ApiPublicOrigin ever changes.
            // Only Published posts block the delete: for drafts and posts in
            // review the FK is SetNull and the body can still be fixed before
            // it goes live.
            var inlinePath = $"/uploads/{storedFilename}";
            var livePostTitles = await db.Posts
                .Where(p => p.Status == PostStatus.Published
                    && (p.FeaturedImageId == id || p.BodyHtml.Contains(inlinePath)))
                .OrderBy(p => p.Title)
                .Select(p => p.Title)
                .ToListAsync();

            if (livePostTitles.Count > 0)
            {
                return Results.Json(new
                {
                    message = BuildInUseMessage(livePostTitles),
                    postTitles = livePostTitles,
                }, statusCode: 409);
            }

            db.MediaAssets.Remove(asset);
            await db.SaveChangesAsync();

            // File goes after the row: if SaveChanges throws, the asset is
            // still listed and still renders, instead of a row pointing at a
            // file that no longer exists.
            var filePath = Path.Combine(env.ContentRootPath, "wwwroot", "uploads", storedFilename);
            if (File.Exists(filePath)) File.Delete(filePath);

            return Results.NoContent();
        })
        .RequireAuthorization("EditorOrOwner");
    }

    private static string BuildInUseMessage(IReadOnlyList<string> postTitles)
    {
        const int maxListed = 3;
        var listed = string.Join(", ", postTitles.Take(maxListed).Select(t => $"\"{t}\""));
        var rest = postTitles.Count > maxListed ? $" and {postTitles.Count - maxListed} more" : "";
        return postTitles.Count == 1
            ? $"This image is used by the published post {listed}. Remove it from that post before deleting."
            : $"This image is used by published posts {listed}{rest}. Remove it from those posts before deleting.";
    }
}
