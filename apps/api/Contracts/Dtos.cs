using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Contracts;

// These records mirror apps/admin/src/lib/types.ts field-for-field. If you change
// a shape here, change it there too (or better: generate types.ts from this API's
// OpenAPI spec via NSwag/openapi-typescript once packages/shared-types exists,
// per CLAUDE.md's monorepo plan, so the two can't drift silently).

public record UserDto(Guid Id, string Name, string Email, Role Role);

public record AuthResponseDto(string AccessToken, UserDto User);

public record ReviewNoteDto(Guid Id, string Comment, string ReviewerName, DateTimeOffset CreatedAt);

public record PostDto(
    Guid Id,
    string Title,
    string Slug,
    string BodyHtml,
    string Excerpt,
    string SeoTitle,
    string MetaDescription,
    Guid? FeaturedImageId,
    Guid CategoryId,
    string CategoryName,
    string CategoryColor,
    string? CategoryFolderColor,
    List<TagRefDto> Tags,
    PostStatus Status,
    Guid AuthorId,
    string AuthorName,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? PublishedAt,
    ReviewNoteDto? LatestReviewNote);

public record MediaAssetDto(Guid Id, string Filename, MediaTag Tag, int Width, int Height, string Url);

public record LoginRequest(string Email, string Password);

public record CreatePostRequest(
    string? Title,
    string? BodyHtml,
    string? Excerpt,
    string? SeoTitle,
    string? MetaDescription,
    Guid? FeaturedImageId,
    Guid? CategoryId,
    PostStatus? Status,
    List<Guid>? TagIds = null);

public record UpdatePostRequest(
    string? Title,
    string? BodyHtml,
    string? Excerpt,
    string? SeoTitle,
    string? MetaDescription,
    Guid? FeaturedImageId,
    Guid? CategoryId,
    PostStatus? Status,
    List<Guid>? TagIds = null);

public record RequestChangesRequest(string Comment);

public record CreateMediaRequest(string Filename, string DataUrl, MediaTag Tag, int Width, int Height);

// PostCount isn't a column on Category — it's computed per-request (how many
// posts currently reference this category), which is also what decides whether
// a hard DELETE is allowed. See CategoryEndpoints.
public record CategoryDto(Guid Id, string Name, string Slug, string Description, string Color, string? FolderColor, int Position, bool IsVisible, bool IsDeleted, int PostCount);

public record TagDto(Guid Id, string Name, string Slug, int PostCount);

public record TagRefDto(Guid Id, string Name, string Slug);

public record PublicTagRefDto(string Name, string Slug);

public record PublicTagDto(string Name, string Slug, int PostCount);

public record CreateTagRequest(string Name);

public record UpdateTagRequest(string? Name, string? Slug);

public record MergeTagRequest(Guid TargetTagId);

public record CreateCategoryRequest(string Name, string Slug, string Description, string Color, int? Position, string? FolderColor = null);

// Same partial-update shape as UpdatePostRequest: only fields you send get
// changed. IsVisible and IsDeleted are separate flags on purpose (see
// Category.cs) — toggling one doesn't touch the other. There's no separate
// "restore" endpoint; setting IsDeleted back to false via this same PATCH does it.
// Description/Color/Position default to null (unlike Name/Slug/IsVisible/
// IsDeleted above, an intentional asymmetry) purely so every existing
// 4-positional-arg test call site (new UpdateCategoryRequest(a, b, c, d))
// keeps compiling without being touched by this change. FolderColor joins
// them at the end for the same reason.
public record UpdateCategoryRequest(
    string? Name, string? Slug, bool? IsVisible, bool? IsDeleted,
    string? Description = null, string? Color = null, int? Position = null, string? FolderColor = null);

public record AuthorApplicationDto(
    Guid Id, string Name, string Email, string Pitch, ApplicationStatus Status,
    DateTimeOffset SubmittedAt, DateTimeOffset? ReviewedAt, string? DevInviteUrl = null);

public record CreateAuthorApplicationRequest(string Name, string Email, string Pitch);

public record CreateDirectAuthorRequest(string Name, string Email);

public record AcceptInviteRequest(string Token, string Password);

public record ManagedUserDto(Guid Id, string Name, string Email, Role Role, bool IsActive, DateTimeOffset CreatedAt, string? Bio);

public record UserDeletionImpactDto(int PostCount, int MediaCount, int ReviewNoteCount, int AffectedOtherPostCount);

public record DirectAddAuthorResponseDto(ManagedUserDto User, string? DevInviteUrl);

public record UpdateUserRequest(Role? Role, bool? IsActive, string? Bio = null);

public record SiteSettingsDto(Guid Id, string SiteTitle, string Tagline, string DefaultMetaDescription, string LinkedInUrl, string XUrl);

public record UpdateSiteSettingsRequest(string SiteTitle, string Tagline, string DefaultMetaDescription, string LinkedInUrl, string XUrl);

public record ActivityEventDto(Guid Id, string ActorName, string Action, DateTimeOffset CreatedAt);

public record PublicPostDto(
    string Slug, string Title, string BodyHtml, string Excerpt, string SeoTitle, string MetaDescription,
    string? FeaturedImageUrl, string CategorySlug, string CategoryName, string CategoryColor, string? CategoryFolderColor,
    List<PublicTagRefDto> Tags,
    string AuthorHandle, string AuthorName, DateTimeOffset PublishedAt, int ReadingMinutes, int DispatchNumber);

public record PublicAuthorDto(string Handle, string Name, string? Bio, DateTimeOffset CreatedAt);

public record PublicCategoryDto(string Name, string Slug, string Description, string Color, string? FolderColor, int Position, int PostCount);

public static class Mapping
{
    public static UserDto ToDto(this ApplicationUser u) => new(u.Id, u.Name, u.Email, u.Role);

    // Unlike AuthorName below (genuinely empty on create — no ApplicationUser
    // is ever loaded anywhere in that flow), a just-created post's Tags comes
    // back correctly POPULATED even though post.PostTags is never explicitly
    // re-Include()d after PostEndpoints' POST handler does its
    // AddRange+SaveChangesAsync for the join rows. This works because the
    // Tag entities were already loaded (and are still tracked) in the same
    // AppDbContext via the TagIds validation query, so when the new PostTag
    // rows are added, EF Core's automatic relationship fixup wires up
    // post.PostTags (and each PostTag.Tag navigation) against those tracked
    // Tag/Post entities in-memory, with no extra query needed.
    public static PostDto ToDto(this Post p) => new(
        p.Id, p.Title, p.Slug, p.BodyHtml, p.Excerpt, p.SeoTitle, p.MetaDescription,
        p.FeaturedImageId, p.CategoryId, p.Category?.Name ?? "", p.Category?.Color ?? "", p.Category?.FolderColor,
        p.PostTags.Select(pt => new TagRefDto(pt.Tag!.Id, pt.Tag.Name, pt.Tag.Slug)).ToList(),
        p.Status, p.AuthorId, p.Author?.Name ?? "", p.UpdatedAt, p.PublishedAt,
        p.ReviewNotes.OrderByDescending(r => r.CreatedAt).FirstOrDefault() is { } latest
            ? new ReviewNoteDto(latest.Id, latest.Comment, latest.Reviewer?.Name ?? "", latest.CreatedAt)
            : null);

    public static MediaAssetDto ToDto(this MediaAsset m) => new(m.Id, m.Filename, m.Tag, m.Width, m.Height, m.Url);

    public static CategoryDto ToDto(this Category c, int postCount) =>
        new(c.Id, c.Name, c.Slug, c.Description, c.Color, c.FolderColor, c.Position, c.IsVisible, c.IsDeleted, postCount);

    public static TagDto ToDto(this Tag t, int postCount) => new(t.Id, t.Name, t.Slug, postCount);

    public static PublicTagDto ToPublicDto(this Tag t, int postCount) => new(t.Name, t.Slug, postCount);

    public static AuthorApplicationDto ToDto(this AuthorApplication a) =>
        new(a.Id, a.Name, a.Email, a.Pitch, a.Status, a.SubmittedAt, a.ReviewedAt);

    public static ManagedUserDto ToManagedDto(this ApplicationUser u) =>
        new(u.Id, u.Name, u.Email, u.Role, u.IsActive, u.CreatedAt, u.Bio);

    public static SiteSettingsDto ToDto(this SiteSettings s) =>
        new(s.Id, s.SiteTitle, s.Tagline, s.DefaultMetaDescription, s.LinkedInUrl, s.XUrl);

    public static ActivityEventDto ToDto(this ActivityLogEntry e) =>
        new(e.Id, e.Actor?.Name ?? "Deleted user", e.Action, e.CreatedAt);

    public static PublicPostDto ToPublicDto(this Post p, int dispatchNumber) => new(
        p.Slug, p.Title, p.BodyHtml, p.Excerpt, p.SeoTitle, p.MetaDescription,
        p.FeaturedImage?.Url, p.Category?.Slug ?? "", p.Category?.Name ?? "", p.Category?.Color ?? "", p.Category?.FolderColor,
        p.PostTags.Select(pt => new PublicTagRefDto(pt.Tag!.Name, pt.Tag.Slug)).ToList(),
        p.Author?.Handle ?? "", p.Author?.Name ?? "",
        p.PublishedAt!.Value, EstimateReadingMinutes(p.BodyHtml), dispatchNumber);

    public static PublicAuthorDto ToPublicDto(this ApplicationUser u) => new(u.Handle, u.Name, u.Bio, u.CreatedAt);

    public static PublicCategoryDto ToPublicDto(this Category c, int postCount) =>
        new(c.Name, c.Slug, c.Description, c.Color, c.FolderColor, c.Position, postCount);

    // Same word-count/200wpm formula as apps/admin/src/lib/formatting.ts's
    // estimateReadTime — computed here so the blog never has to duplicate it.
    private static int EstimateReadingMinutes(string bodyHtml)
    {
        var text = System.Text.RegularExpressions.Regex.Replace(bodyHtml, "<[^>]*>", " ");
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Round(words / 200.0));
    }
}
