using System.Security.Claims;
using System.Text.RegularExpressions;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class MediaEndpoints
{
    private const long MaxUploadBytes = 8 * 1024 * 1024;

    private static readonly Dictionary<string, string> AllowedImageExtensions = new()
    {
        ["image/png"] = "png",
        ["image/jpeg"] = "jpg",
        ["image/webp"] = "webp",
        ["image/gif"] = "gif",
        ["image/svg+xml"] = "svg",
    };

    private static readonly Regex DataUrlPattern =
        new(@"^data:(?<mime>image/[\w.+-]+);base64,(?<data>.+)$", RegexOptions.Singleline | RegexOptions.Compiled);

    public static void MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media").WithTags("Media").RequireAuthorization();

        // GET /api/media?search=... — matches mocks/handlers/media.ts.
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

        // POST /api/media — decodes the client's data URL and writes it to disk
        // under wwwroot/uploads (served by app.UseStaticFiles() in Program.cs),
        // returning a real static URL instead of persisting the data: URI itself.
        group.MapPost("/", async (
            CreateMediaRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            IWebHostEnvironment env,
            HttpRequest httpRequest) =>
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

            var uploadsDir = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");

            var storedFilename = $"{Guid.CreateVersion7()}.{extension}";
            await File.WriteAllBytesAsync(Path.Combine(uploadsDir, storedFilename), bytes);

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")!);

            var asset = new MediaAsset
            {
                Filename = request.Filename,
                Tag = request.Tag,
                Width = request.Width,
                Height = request.Height,
                Url = $"{httpRequest.Scheme}://{httpRequest.Host}/uploads/{storedFilename}",
                UploadedById = userId,
            };

            db.MediaAssets.Add(asset);
            await db.SaveChangesAsync();
            return Results.Created($"/api/media/{asset.Id}", asset.ToDto());
        });
    }
}
