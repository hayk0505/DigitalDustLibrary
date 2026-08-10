namespace DigitalDustLibrary.Api.Models;

/// <summary>
/// Backs the httpOnly refresh cookie decided in Admin_Panel_Build_Spec.md
/// (in-memory access token + httpOnly refresh cookie, not localStorage). Only
/// the SHA-256 hash of the raw token is stored — the raw value only ever exists
/// in the cookie itself — so a DB leak doesn't hand out usable refresh tokens.
/// Rotated on every use (old row revoked, new row+cookie issued) rather than
/// reused, which limits the damage window if a refresh cookie is ever stolen.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
