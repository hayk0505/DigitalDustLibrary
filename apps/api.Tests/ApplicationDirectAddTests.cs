using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class ApplicationDirectAddTests(ApiFactory factory)
{
    [Fact]
    public async Task DirectAdd_ValidRequest_CreatesInactiveUserAndSendsEmail()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var email = $"direct-{suffix}@example.com";

        var response = await editor.PostAsJsonAsync(
            "/api/applications/direct", new CreateDirectAuthorRequest($"Direct Test {suffix}", email));

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<DirectAddAuthorResponseDto>(AuthHelper.JsonOptions);
        Assert.Equal(Role.Author, body!.User.Role);
        Assert.False(body.User.IsActive);
        Assert.Equal(email, body.User.Email);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createdUser = await db.Users.SingleAsync(u => u.Email == email);
        var inviteToken = await db.InviteTokens.SingleAsync(t => t.UserId == createdUser.Id);
        Assert.True(inviteToken.IsActive);

        var emailSender = (LoggingEmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var sentEmail = emailSender.Sent.Last(e => e.To == email);
        Assert.Contains("invited", sentEmail.Subject, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DirectAdd_ResponseIncludesDevInviteUrl_WhenLoggingSenderActive()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var email = $"devurl-{suffix}@example.com";

        var response = await editor.PostAsJsonAsync(
            "/api/applications/direct", new CreateDirectAuthorRequest($"Dev Url Test {suffix}", email));

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<DirectAddAuthorResponseDto>(AuthHelper.JsonOptions);
        Assert.NotNull(body!.DevInviteUrl);
        Assert.Contains("/set-password?token=", body.DevInviteUrl);
    }

    [Fact]
    public async Task DirectAdd_EmailAlreadyRegistered_Returns409()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PostAsJsonAsync(
            "/api/applications/direct", new CreateDirectAuthorRequest("Duplicate", AuthHelper.OwnerEmail));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await db.Users.CountAsync(u => u.Email == AuthHelper.OwnerEmail));
    }

    [Fact]
    public async Task DirectAdd_LogsActivityEntry()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();

        var response = await editor.PostAsJsonAsync(
            "/api/applications/direct",
            new CreateDirectAuthorRequest($"Activity Test {suffix}", $"activity-{suffix}@example.com"));
        response.EnsureSuccessStatusCode();

        var activityResponse = await owner.GetAsync("/api/activity");
        var entries = await activityResponse.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.Contains(entries!, e => e.Action == $"added Activity Test {suffix} directly as an author");
    }

    [Fact]
    public async Task DirectAdd_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/applications/direct", new CreateDirectAuthorRequest("No Auth", "no-auth@example.com"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DirectAdd_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PostAsJsonAsync(
            "/api/applications/direct", new CreateDirectAuthorRequest("As Author", "as-author@example.com"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
