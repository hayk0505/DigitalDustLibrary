namespace DigitalDustLibrary.Api.Models;

public class Post
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public string BodyHtml { get; set; } = "";
    public string Excerpt { get; set; } = "";
    public string SeoTitle { get; set; } = "";
    public string MetaDescription { get; set; } = "";
    public Guid? FeaturedImageId { get; set; }
    public MediaAsset? FeaturedImage { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;

    public Guid AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }

    // Every post belongs to exactly one Category — non-nullable, same
    // guarantee the old Pillar enum had. Nav property stays nullable (like
    // Author above) since EF only populates it when .Include()d.
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PublishedAt { get; set; }

    public List<ReviewNote> ReviewNotes { get; set; } = [];
    public List<PostTag> PostTags { get; set; } = [];
}
