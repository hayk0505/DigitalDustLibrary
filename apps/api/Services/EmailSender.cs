using System.Net.Http.Json;
using Resend;

namespace DigitalDustLibrary.Api.Services;

public class ResendOptions
{
    public const string SectionName = "Resend";
    public string ApiKey { get; set; } = "";
    public string FromAddress { get; set; } = "";
}

// FromAddress/FromName are split (unlike Resend's single "Name <email>"
// string) because Brevo's API wants sender.email and sender.name as
// separate JSON fields — see BrevoEmailSender.SendAsync below.
public class BrevoOptions
{
    public const string SectionName = "Brevo";
    public string ApiKey { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "Digital Dust Library";
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

// Real sends via Brevo's REST API (https://api.brevo.com/v3/smtp/email) —
// the provider actually used in production (see CLAUDE.md/deployment notes:
// Resend's free tier caps out at 1 verified domain, already used by
// haykbaroyan.com, so this project uses Brevo instead — 300 emails/day free,
// no domain cap). Plain HttpClient rather than a NuGet SDK: Brevo's send
// endpoint is a single simple POST, not worth a dependency for. Registered
// as a typed client (see Program.cs's AddHttpClient<BrevoEmailSender>) so
// the base address and api-key header are set up once at DI registration
// time rather than per call.
public class BrevoEmailSender(HttpClient http, BrevoOptions options) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string html)
    {
        var payload = new
        {
            sender = new { email = options.FromAddress, name = options.FromName },
            to = new[] { new { email = toEmail } },
            subject,
            htmlContent = html,
        };

        var response = await http.PostAsJsonAsync("v3/smtp/email", payload);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to send email via Brevo: {(int)response.StatusCode} {response.StatusCode} — {body}");
        }
    }
}
