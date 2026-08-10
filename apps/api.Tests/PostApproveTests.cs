using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PostApproveTests(ApiFactory factory)
{
    [Fact]
    public async Task Approve_PendingReviewPost_PublishesAndSetsPublishedAt()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Approve Author {suffix}", $"approve-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Approve Me {suffix}", PostStatus.PendingReview);

        var response = await editor.PostAsync($"/api/posts/{post.Id}/approve", null);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(PostStatus.Published, updated!.Status);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbPost = await db.Posts.SingleAsync(p => p.Id == post.Id);
        Assert.NotNull(dbPost.PublishedAt);
    }

    [Fact]
    public async Task Approve_UnknownId_Returns404()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PostAsync($"/api/posts/{Guid.NewGuid()}/approve", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Approve_DraftPost_Returns409()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Draft Author {suffix}", $"draft-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Still Draft {suffix}", PostStatus.Draft);

        var response = await editor.PostAsync($"/api/posts/{post.Id}/approve", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Approve_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync($"/api/posts/{Guid.NewGuid()}/approve", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Approve_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PostAsync($"/api/posts/{Guid.NewGuid()}/approve", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Approve_ResponseIncludesPublishedAt()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"PubAt Author {suffix}", $"pubat-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"PubAt Me {suffix}", PostStatus.PendingReview);

        var response = await editor.PostAsync($"/api/posts/{post.Id}/approve", null);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.NotNull(updated!.PublishedAt);
    }

    [Fact]
    public async Task Approve_LogsActivityEntry()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Log Author {suffix}", $"log-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Log Me {suffix}", PostStatus.PendingReview);

        var response = await editor.PostAsync($"/api/posts/{post.Id}/approve", null);
        response.EnsureSuccessStatusCode();

        var activityResponse = await owner.GetAsync("/api/activity");
        var entries = await activityResponse.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.Contains(entries!, e => e.Action == $"published \"Log Me {suffix}\"");
    }
}
