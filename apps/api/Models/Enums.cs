namespace DigitalDustLibrary.Api.Models;

// Enum names are PascalCase in C#; a global JsonStringEnumConverter with
// JsonNamingPolicy.SnakeCaseLower (configured in Program.cs) serializes them
// to match apps/admin/src/lib/types.ts exactly:
//   Role.Owner -> "owner", PostStatus.PendingReview -> "pending_review",
//   MediaTag.OgImage -> "og_image".
// (Pillar was removed 2026-08-12 in favor of the free-form Category
// taxonomy — see Models/Category.cs — so don't go looking for it here.)
//
// [Authorize(Roles = "...")] compares against the raw claim value, which we
// set to the PascalCase enum name (see AuthEndpoints) — separate from the
// JSON wire format, deliberately, so a policy check isn't coupled to the
// serialization naming policy.

public enum Role
{
    Author,
    Editor,
    Owner,
}

// NOTE: 'Scheduled' intentionally omitted — deferred per Admin_Panel_Build_Spec.md
// (decision 2026-07-13). Add it here (and to apps/admin/src/lib/types.ts) together
// if/when scheduled publishing is built.
public enum PostStatus
{
    Draft,
    PendingReview,
    ChangesRequested,
    Published,
}

public enum MediaTag
{
    Featured,
    Inline,
    OgImage,
    Avatar,
}
