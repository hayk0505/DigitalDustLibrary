namespace DigitalDustLibrary.Api.Models;

/// <summary>
/// Schema-only for now — no /categories endpoints yet (Categories screen is a
/// deferred phase per docs/superpowers/specs/2026-07-13-admin-panel-phase1-design.md).
/// Laid down now, per CLAUDE.md's "add the FK/table early, avoid a painful
/// migration later" principle. Encodes the hide/soft-delete/blocked-hard-delete
/// rule from Functional_Overview_for_Design.md: IsVisible toggles public-site
/// visibility independent of deletion; IsDeleted is the soft-delete flag; a true
/// hard DELETE should be rejected at the API layer whenever any Post still
/// references this category (checked at delete-time, not enforced by the schema).
/// </summary>
public class Category
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public bool IsPillar { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
