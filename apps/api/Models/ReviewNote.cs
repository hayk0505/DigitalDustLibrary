namespace DigitalDustLibrary.Api.Models;

/// <summary>
/// One row per "Request changes" action, per CLAUDE.md's review-notes model
/// (post_id, reviewer_id, comment, created_at). apps/admin's Phase 1 only reads
/// <c>Post.latestReviewNote</c> (nullable, most-recent-first) — the Review
/// Queue/Review Detail screens that let an Editor/Owner actually create these
/// are a later phase (not yet built), so there's no write endpoint for this yet,
/// just the schema and the read-shape Post Editor already expects.
/// </summary>
public class ReviewNote
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid PostId { get; set; }
    public Post? Post { get; set; }
    public Guid ReviewerId { get; set; }
    public ApplicationUser? Reviewer { get; set; }
    public required string Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
