namespace DigitalDustLibrary.Api.Models;

/// <summary>
/// Deliberately not using full ASP.NET Core Identity (UserManager/SignInManager/
/// IdentityDbContext) — this app only ever needs one role per user (matches
/// apps/admin's <c>User { role: 'author' | 'editor' | 'owner' }</c>, not a
/// multi-role claims system) and JWT-only auth, not cookie/external-login
/// infrastructure. Using <see cref="Microsoft.AspNetCore.Identity.PasswordHasher{TUser}"/>
/// directly (ships in the ASP.NET Core shared framework, no extra package) for
/// battle-tested PBKDF2 hashing, with a plain EF Core entity/table otherwise.
/// Simpler, fewer moving parts, same security properties for what this needs.
/// </summary>
public class ApplicationUser
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public required string Handle { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public Role Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Short author bio shown on the public blog's Author File sidebar card
    // and author page. Nullable/admin-optional — no bio set just means the
    // card omits that line rather than showing a placeholder.
    public string? Bio { get; set; }
}
