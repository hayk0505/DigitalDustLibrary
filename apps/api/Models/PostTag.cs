namespace DigitalDustLibrary.Api.Models;

// Explicit join entity (not an EF Core "skip navigation" List<Tag>-on-both-
// sides many-to-many) — kept as its own addressable DbSet so tag assignment
// and TagEndpoints.cs's merge logic can operate on join rows directly via
// db.PostTags, the same "every entity goes straight to its own DbSet"
// convention ReviewNote already follows in this codebase (see the EF Core
// gotcha noted on Post.cs/ReviewNote.cs). Composite key (PostId, TagId) —
// see AppDbContext's OnModelCreating.
public class PostTag
{
    public Guid PostId { get; set; }
    public Post? Post { get; set; }
    public Guid TagId { get; set; }
    public Tag? Tag { get; set; }
}
