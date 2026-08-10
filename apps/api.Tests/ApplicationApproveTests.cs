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
public class ApplicationApproveTests(ApiFactory factory)
{
    [Fact]
    public async Task Approve_PendingApplication_CreatesInactiveUserAndSendsEmail()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var email = $"approve-{suffix}@example.com";
        var application = await ApplicationTestHelpers.CreatePendingApplicationAsync(
            factory, $"Approve Test {suffix}", email, "Pitch text");

        var response = await editor.PostAsync($"/api/applications/{application.Id}/approve", null);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<AuthorApplicationDto>(AuthHelper.JsonOptions);
        Assert.Equal(ApplicationStatus.Approved, updated!.Status);
        Assert.NotNull(updated.ReviewedAt);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createdUser = await db.Users.SingleAsync(u => u.Email == email);
        Assert.Equal(Role.Author, createdUser.Role);
        Assert.False(createdUser.IsActive);
        var inviteToken = await db.InviteTokens.SingleAsync(t => t.UserId == createdUser.Id);
        Assert.True(inviteToken.IsActive);

        var emailSender = (LoggingEmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var sentEmail = emailSender.Sent.Last(e => e.To == email);
        Assert.Contains("approved", sentEmail.Subject, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Approve_UnknownId_Returns404()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PostAsync($"/api/applications/{Guid.NewGuid()}/approve", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Approve_AlreadyReviewedApplication_Returns409()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var application = await ApplicationTestHelpers.CreatePendingApplicationAsync(
            factory, $"Twice {suffix}", $"twice-{suffix}@example.com", "Pitch");
        var first = await editor.PostAsync($"/api/applications/{application.Id}/approve", null);
        first.EnsureSuccessStatusCode();

        var second = await editor.PostAsync($"/api/applications/{application.Id}/approve", null);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Approve_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync($"/api/applications/{Guid.NewGuid()}/approve", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Approve_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PostAsync($"/api/applications/{Guid.NewGuid()}/approve", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Approve_CreatedUserHasNonEmptyHandle()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var email = $"handle-{suffix}@example.com";
        var application = await ApplicationTestHelpers.CreatePendingApplicationAsync(
            factory, $"Handle Test {suffix}", email, "Pitch");

        var response = await editor.PostAsync($"/api/applications/{application.Id}/approve", null);
        response.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var createdUser = await db.Users.SingleAsync(u => u.Email == email);
        Assert.False(string.IsNullOrEmpty(createdUser.Handle));
        Assert.StartsWith("handle-test", createdUser.Handle);
    }

    [Fact]
    public async Task Approve_LogsActivityEntry()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var application = await ApplicationTestHelpers.CreatePendingApplicationAsync(
            factory, $"Approve Log {suffix}", $"approve-log-{suffix}@example.com", "Pitch");

        var response = await editor.PostAsync($"/api/applications/{application.Id}/approve", null);
        response.EnsureSuccessStatusCode();

        var activityResponse = await owner.GetAsync("/api/activity");
        var entries = await activityResponse.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.Contains(entries!, e => e.Action == $"approved Approve Log {suffix}'s application");
    }
}
