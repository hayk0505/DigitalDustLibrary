namespace DigitalDustLibrary.Api.Models;

public class MediaAsset
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Filename { get; set; }
    public MediaTag Tag { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    /// <summary>
    /// Absolute public URL of the stored file (e.g. http://localhost:5080/uploads/{guid}.png
    /// in dev). The client sends a data URL; MediaEndpoints.cs decodes and writes it to
    /// disk under wwwroot/uploads (served via app.UseStaticFiles()) and stores the
    /// resulting static URL here, not the data: URI itself. Absolute, not relative,
    /// because admin and the API may sit on different origins/subdomains in production
    /// (see CLAUDE.md's hosting section) — a relative path would only resolve correctly
    /// when both happen to share an origin.
    /// </summary>
    public required string Url { get; set; }

    public Guid UploadedById { get; set; }
    public ApplicationUser? UploadedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
