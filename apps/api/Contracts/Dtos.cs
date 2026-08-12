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
    Pillar Pillar,
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
    Pillar? Pillar,
    PostStatus? Status);

public record UpdatePostRequest(
    string? Title,
    string? BodyHtml,
    string? Excerpt,
    string? SeoTitle,
    string? MetaDescription,
    Guid? FeaturedImageId,
    Pillar? Pillar,
    PostStatus? Status);

public record RequestChangesRequest(string Comment);

public record CreateMediaRequest(string Filename, string DataUrl, MediaTag Tag, int Width, int Height);

// PostCount isn't a column on Category — it's computed per-request (how many
// posts currently reference this category), which is also what decides whether
// a hard DELETE is allowed. See CategoryEndpoints.
public record CategoryDto(Guid Id, string Name, string Slug, bool IsPillar, bool IsVisible, bool IsDeleted, int PostCount);

public record CreateCategoryRequest(string Name, string Slug, bool? IsPillar);

// Same partial-update shape as UpdatePostRequest: only fields you send get
// changed. IsVisible and IsDeleted are separate flags on purpose (see
// Category.cs) — toggling one doesn't touch the other. There's no separate
// "restore" endpoint; setting IsDeleted back to false via this same PATCH does it.
public record UpdateCategoryRequest(string? Name, string? Slug, bool? IsVisible, bool? IsDeleted);

public record AuthorApplicationDto(
    Guid Id, string Name, string Email, string Pitch, ApplicationStatus Status,
    DateTimeOffset SubmittedAt, DateTimeOffset? ReviewedAt, string? DevInviteUrl = null);

public record CreateAuthorApplicationRequest(string Name, string Email, string Pitch);

public record CreateDirectAuthorRequest(string Name, string Email);

public record AcceptInviteRequest(string Token, string Password);

public record ManagedUserDto(Guid Id, string Name, string Email, Role Role, bool IsActive, DateTimeOffset CreatedAt);

public record UserDeletionImpactDto(int PostCount, int MediaCount, int ReviewNoteCount, int AffectedOtherPostCount);

public record DirectAddAuthorResponseDto(ManagedUserDto User, string? DevInviteUrl);

public record UpdateUserRequest(Role? Role, bool? IsActive);

public record SiteSettingsDto(Guid Id, string SiteTitle, string Tagline, string DefaultMetaDescription, string LinkedInUrl, string XUrl);

public record UpdateSiteSettingsRequest(string SiteTitle, string Tagline, string DefaultMetaDescription, string LinkedInUrl, string XUrl);

public record ActivityEventDto(Guid Id, string ActorName, string Action, DateTimeOffset CreatedAt);

public record PublicPostDto(
    string Slug, string Title, string BodyHtml, string Excerpt, string SeoTitle, string MetaDescription,
    string? FeaturedImageUrl, Pillar Pillar, string AuthorHandle, string AuthorName,
    DateTimeOffset PublishedAt, int ReadingMinutes, int DispatchNumber);

public record PublicAuthorDto(string Handle, string Name);

public record PublicCategoryDto(string Name, string Slug);

public static class Mapping
{
    public static UserDto ToDto(this ApplicationUser u) => new(u.Id, u.Name, u.Email, u.Role);

    public static PostDto ToDto(this Post p) => new(
        p.Id, p.Title, p.Slug, p.BodyHtml, p.Excerpt, p.SeoTitle, p.MetaDescription,
        p.FeaturedImageId, p.Pillar, p.Status, p.AuthorId, p.Author?.Name ?? "", p.UpdatedAt, p.PublishedAt,
        p.ReviewNotes.OrderByDescending(r => r.CreatedAt).FirstOrDefault() is { } latest
            ? new ReviewNoteDto(latest.Id, latest.Comment, latest.Reviewer?.Name ?? "", latest.CreatedAt)
            : null);

    public static MediaAssetDto ToDto(this MediaAsset m) => new(m.Id, m.Filename, m.Tag, m.Width, m.Height, m.Url);

    public static CategoryDto ToDto(this Category c, int postCount) =>
        new(c.Id, c.Name, c.Slug, c.IsPillar, c.IsVisible, c.IsDeleted, postCount);

    public static AuthorApplicationDto ToDto(this AuthorApplication a) =>
        new(a.Id, a.Name, a.Email, a.Pitch, a.Status, a.SubmittedAt, a.ReviewedAt);

    public static ManagedUserDto ToManagedDto(this ApplicationUser u) =>
        new(u.Id, u.Name, u.Email, u.Role, u.IsActive, u.CreatedAt);

    public static SiteSettingsDto ToDto(this SiteSettings s) =>
        new(s.Id, s.SiteTitle, s.Tagline, s.DefaultMetaDescription, s.LinkedInUrl, s.XUrl);

    public static ActivityEventDto ToDto(this ActivityLogEntry e) =>
        new(e.Id, e.Actor?.Name ?? "Deleted user", e.Action, e.CreatedAt);

    public static PublicPostDto ToPublicDto(this Post p, int dispatchNumber) => new(
        p.Slug, p.Title, p.BodyHtml, p.Excerpt, p.SeoTitle, p.MetaDescription,
        p.FeaturedImage?.Url, p.Pillar, p.Author?.Handle ?? "", p.Author?.Name ?? "",
        p.PublishedAt!.Value, EstimateReadingMinutes(p.BodyHtml), dispatchNumber);

    public static PublicAuthorDto ToPublicDto(this ApplicationUser u) => new(u.Handle, u.Name);

    public static PublicCategoryDto ToPublicDto(this Category c) => new(c.Name, c.Slug);

    // Same word-count/200wpm formula as apps/admin/src/lib/formatting.ts's
    // estimateReadTime — computed here so the blog never has to duplicate it.
    private static int EstimateReadingMinutes(string bodyHtml)
    {
        var text = System.Text.RegularExpressions.Regex.Replace(bodyHtml, "<[^>]*>", " ");
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Round(words / 200.0));
    }
}
