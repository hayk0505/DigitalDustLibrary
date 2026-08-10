using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class AcceptInviteTests(ApiFactory factory)
{
    private async Task<string> ApproveAndGetInviteTokenAsync(string name, string email)
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var application = await ApplicationTestHelpers.CreatePendingApplicationAsync(factory, name, email, "Pitch");
        var approveResponse = await editor.PostAsync($"/api/applications/{application.Id}/approve", null);
        approveResponse.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var emailSender = (LoggingEmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var sentEmail = emailSender.Sent.Last(e => e.To == email);
        var match = Regex.Match(sentEmail.Html, "token=([^\"&]+)");
        Assert.True(match.Success, "expected the approval email to contain an invite link with a token");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }

    [Fact]
    public async Task AcceptInvite_ValidToken_SetsPasswordActivatesAccountAndLogsIn()
    {
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var email = $"accept-{suffix}@example.com";
        var token = await ApproveAndGetInviteTokenAsync($"Accept Test {suffix}", email);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/accept-invite",
            new AcceptInviteRequest(token, "a-real-password-123"));

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(AuthHelper.JsonOptions);
        Assert.Equal(email, auth!.User.Email);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "a-real-password-123"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task AcceptInvite_UnknownToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/accept-invite",
            new AcceptInviteRequest("not-a-real-token", "a-real-password-123"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AcceptInvite_AlreadyRedeemedToken_Returns401OnSecondUse()
    {
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var email = $"reused-{suffix}@example.com";
        var token = await ApproveAndGetInviteTokenAsync($"Reused Test {suffix}", email);
        var client = factory.CreateClient();
        var first = await client.PostAsJsonAsync("/api/auth/accept-invite",
            new AcceptInviteRequest(token, "first-password-123"));
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsJsonAsync("/api/auth/accept-invite",
            new AcceptInviteRequest(token, "second-password-123"));

        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
    }
}
