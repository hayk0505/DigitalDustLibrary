using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Contracts;


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
    ReviewNoteDto? LatestReviewNote,
    PostFormat Format);

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
    List<Guid>? TagIds = null,
    PostFormat? Format = null);

public record UpdatePostRequest(
    string? Title,
    string? BodyHtml,
    string? Excerpt,
    string? SeoTitle,
    string? MetaDescription,
    Guid? FeaturedImageId,
    Guid? CategoryId,
    PostStatus? Status,
    List<Guid>? TagIds = null,
    PostFormat? Format = null);

public record RequestChangesRequest(string Comment);

public record CreateMediaRequest(string Filename, string DataUrl, MediaTag Tag, int Width, int Height);

public record CategoryDto(Guid Id, string Name, string Slug, string Description, string Color, string? FolderColor, int Position, bool IsVisible, bool IsDeleted, int PostCount);

public record TagDto(Guid Id, string Name, string Slug, int PostCount);

public record TagRefDto(Guid Id, string Name, string Slug);

public record PublicTagRefDto(string Name, string Slug);

public record PublicTagDto(string Name, string Slug, int PostCount);

public record CreateTagRequest(string Name);

public record UpdateTagRequest(string? Name, string? Slug);

public record MergeTagRequest(Guid TargetTagId);

public record CreateCategoryRequest(string Name, string Slug, string Description, string Color, int? Position, string? FolderColor = null);

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
    string AuthorHandle, string AuthorName, DateTimeOffset PublishedAt, int ReadingMinutes, int DispatchNumber,
    PostFormat Format);

public record PublicAuthorDto(string Handle, string Name, string? Bio, DateTimeOffset CreatedAt);

public record PublicCategoryDto(string Name, string Slug, string Description, string Color, string? FolderColor, int Position, int PostCount);

public static class Mapping
{
    public static UserDto ToDto(this ApplicationUser u) => new(u.Id, u.Name, u.Email, u.Role);

    public static PostDto ToDto(this Post p) => new(
        p.Id, p.Title, p.Slug, p.BodyHtml, p.Excerpt, p.SeoTitle, p.MetaDescription,
        p.FeaturedImageId, p.CategoryId, p.Category?.Name ?? "", p.Category?.Color ?? "", p.Category?.FolderColor,
        p.PostTags.Select(pt => new TagRefDto(pt.Tag!.Id, pt.Tag.Name, pt.Tag.Slug)).ToList(),
        p.Status, p.AuthorId, p.Author?.Name ?? "", p.UpdatedAt, p.PublishedAt,
        p.ReviewNotes.OrderByDescending(r => r.CreatedAt).FirstOrDefault() is { } latest
            ? new ReviewNoteDto(latest.Id, latest.Comment, latest.Reviewer?.Name ?? "", latest.CreatedAt)
            : null,
        p.Format);

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
        p.PublishedAt!.Value, EstimateReadingMinutes(p.BodyHtml), dispatchNumber, p.Format);

    public static PublicAuthorDto ToPublicDto(this ApplicationUser u) => new(u.Handle, u.Name, u.Bio, u.CreatedAt);

    public static PublicCategoryDto ToPublicDto(this Category c, int postCount) =>
        new(c.Name, c.Slug, c.Description, c.Color, c.FolderColor, c.Position, postCount);

    private static int EstimateReadingMinutes(string bodyHtml)
    {
        var text = System.Text.RegularExpressions.Regex.Replace(bodyHtml, "<[^>]*>", " ");
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Round(words / 200.0));
    }
}
