namespace DigitalDustLibrary.Api.Models;

/// <summary>
/// Append-only — entries are never updated or deleted. Action is a free-text,
/// human-readable string built at the call site (e.g. "published \"Title\""),
/// not a structured enum — matches apps/admin/src/lib/types.ts's ActivityEvent
/// type exactly, so the frontend stays a dumb renderer.
/// </summary>
public class ActivityLogEntry
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid? ActorId { get; set; }
    public ApplicationUser? Actor { get; set; }
    public required string Action { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
