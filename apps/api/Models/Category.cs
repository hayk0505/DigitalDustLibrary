namespace DigitalDustLibrary.Api.Models;

/// <summary>
/// The one taxonomy for posts (replaces the old fixed-3 Pillar enum — see
/// docs/superpowers/specs/2026-08-12-category-taxonomy-design.md). Any
/// number of categories, admin-managed: name/slug/description/color/order,
/// hide/soft-delete/blocked-hard-delete (IsVisible toggles public-site
/// visibility independent of deletion; IsDeleted is the soft-delete flag; a
/// true hard DELETE is rejected at the API layer whenever any Post still
/// references this category).
/// </summary>
public class Category
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string Description { get; set; }
    public required string Color { get; set; }

    // Muted tone for the blog's file-folder category tabs — deliberately
    // separate from Color (the bright accent used for dots/hover elsewhere
    // on the site). Nullable and admin-optional: unset categories fall back
    // to a deterministic hashed tone client-side (see apps/blog's
    // category-visuals.ts), so this never blocks category creation.
    public string? FolderColor { get; set; }

    public int Position { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
