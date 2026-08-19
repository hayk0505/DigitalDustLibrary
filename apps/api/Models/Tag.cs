namespace DigitalDustLibrary.Api.Models;

// Deliberately minimal — no Color/Description/Position/IsVisible/IsDeleted
// like Category. Tags are free-typed by any Author while drafting (see
// TagEndpoints.cs's POST get-or-create) and render as plain-text pill
// badges; rename/merge/delete cover the cleanup Category needs
// visibility/soft-delete state for. See
// docs/superpowers/specs/2026-08-17-tags-design.md.
public class Tag
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
