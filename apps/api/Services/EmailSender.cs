using Resend;

namespace DigitalDustLibrary.Api.Services;

public class ResendOptions
{
    public const string SectionName = "Resend";
    public string ApiKey { get; set; } = "";
    public string FromAddress { get; set; } = "";
}

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string html);
}

public record SentEmail(string To, string Subject, string Html);

// Used whenever Resend:ApiKey is empty/missing — the default in dev
// (appsettings.Development.json intentionally has no real key committed).
// Logs instead of sending, and keeps an in-memory record so tests can assert
// on it without a real Resend account.
public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    private readonly List<SentEmail> _sent = [];
    public IReadOnlyList<SentEmail> Sent => _sent;

    public Task SendAsync(string toEmail, string subject, string html)
    {
        _sent.Add(new SentEmail(toEmail, subject, html));
        logger.LogInformation(
            "Email not actually sent (no Resend:ApiKey configured) — to {To}: {Subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}

// Real sends via the Resend SDK. Used when Resend:ApiKey is configured — not
// yet exercised against a real Resend account as of this writing; verify
// end-to-end once a real API key and verified sending domain exist.
public class ResendEmailSender(IResend resend, ResendOptions options) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string html)
    {
        var message = new EmailMessage
        {
            From = options.FromAddress,
            To = { toEmail },
            Subject = subject,
            HtmlBody = html,
        };

        var response = await resend.EmailSendAsync(message);
        if (!response.Success)
        {
            throw new InvalidOperationException($"Failed to send email via Resend: {response.Exception?.Message}");
        }
    }
}
