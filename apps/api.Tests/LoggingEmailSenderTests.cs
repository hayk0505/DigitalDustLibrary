using DigitalDustLibrary.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace DigitalDustLibrary.Api.Tests;

public class LoggingEmailSenderTests
{
    [Fact]
    public async Task SendAsync_RecordsEmailInSentList()
    {
        var sender = new LoggingEmailSender(NullLogger<LoggingEmailSender>.Instance);

        await sender.SendAsync("test@example.com", "Subject", "<p>Body</p>");

        var sent = Assert.Single(sender.Sent);
        Assert.Equal("test@example.com", sent.To);
        Assert.Equal("Subject", sent.Subject);
        Assert.Equal("<p>Body</p>", sent.Html);
    }
}
