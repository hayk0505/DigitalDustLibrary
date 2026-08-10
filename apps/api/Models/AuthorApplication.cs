namespace DigitalDustLibrary.Api.Models;

public enum ApplicationStatus
{
    Pending,
    Approved,
    Rejected,
}

/// <summary>
/// Schema-only for now — the public application form and the Applications review
/// screen are both deferred phases. Deliberately NOT a FK to ApplicationUser:
/// applicants aren't real accounts until approved (per CLAUDE.md). On approval,
/// the (not-yet-built) approval endpoint should create a real ApplicationUser
/// with Role.Author and send the Resend "set your password" email.
/// </summary>
public class AuthorApplication
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Pitch { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReviewedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public ApplicationUser? ReviewedByUser { get; set; }
}
