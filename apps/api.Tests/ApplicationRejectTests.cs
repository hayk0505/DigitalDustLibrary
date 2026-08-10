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
public class ApplicationRejectTests(ApiFactory factory)
{
    [Fact]
    public async Task Reject_PendingApplication_MarksRejectedAndSendsEmailWithoutCreatingUser()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var email = $"reject-{suffix}@example.com";
        var application = await ApplicationTestHelpers.CreatePendingApplicationAsync(
            factory, $"Reject Test {suffix}", email, "Pitch text");

        var response = await editor.PostAsync($"/api/applications/{application.Id}/reject", null);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<AuthorApplicationDto>(AuthHelper.JsonOptions);
        Assert.Equal(ApplicationStatus.Rejected, updated!.Status);
        Assert.NotNull(updated.ReviewedAt);

        using var scope = factory.Services.CreateScope();
        var emailSender = (LoggingEmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var sentEmail = emailSender.Sent.Last(e => e.To == email);
        Assert.False(string.IsNullOrEmpty(sentEmail.Subject));

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.Users.AnyAsync(u => u.Email == email));
    }

    [Fact]
    public async Task Reject_UnknownId_Returns404()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PostAsync($"/api/applications/{Guid.NewGuid()}/reject", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Reject_AlreadyReviewedApplication_Returns409()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var application = await ApplicationTestHelpers.CreatePendingApplicationAsync(
            factory, $"Twice {suffix}", $"twice-reject-{suffix}@example.com", "Pitch");
        var first = await editor.PostAsync($"/api/applications/{application.Id}/reject", null);
        first.EnsureSuccessStatusCode();

        var second = await editor.PostAsync($"/api/applications/{application.Id}/reject", null);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Reject_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync($"/api/applications/{Guid.NewGuid()}/reject", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Reject_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PostAsync($"/api/applications/{Guid.NewGuid()}/reject", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reject_LogsActivityEntry()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var application = await ApplicationTestHelpers.CreatePendingApplicationAsync(
            factory, $"Reject Log {suffix}", $"reject-log-{suffix}@example.com", "Pitch");

        var response = await editor.PostAsync($"/api/applications/{application.Id}/reject", null);
        response.EnsureSuccessStatusCode();

        var activityResponse = await owner.GetAsync("/api/activity");
        var entries = await activityResponse.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.Contains(entries!, e => e.Action == $"rejected Reject Log {suffix}'s application");
    }
}
