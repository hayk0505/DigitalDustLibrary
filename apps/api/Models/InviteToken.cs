namespace DigitalDustLibrary.Api.Models;

/// <summary>
/// Backs the "set your password" invite link emailed on application approval
/// (see ApplicationEndpoints' /approve route). Mirrors RefreshToken.cs exactly:
/// only the SHA-256 hash of the raw token is stored, the raw value only ever
/// exists in the emailed link, and it's single-use (RevokedAt set on redemption
/// in AuthEndpoints' /accept-invite route).
/// </summary>
public class InviteToken
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
