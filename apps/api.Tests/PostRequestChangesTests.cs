using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PostRequestChangesTests(ApiFactory factory)
{
    [Fact]
    public async Task RequestChanges_PendingReviewPost_SetsStatusAndCreatesReviewNote()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"RC Author {suffix}", $"rc-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Needs Work {suffix}", PostStatus.PendingReview);

        var response = await editor.PostAsJsonAsync($"/api/posts/{post.Id}/request-changes",
            new RequestChangesRequest("Please add a stronger intro."));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(PostStatus.ChangesRequested, updated!.Status);
        Assert.NotNull(updated.LatestReviewNote);
        Assert.Equal("Please add a stronger intro.", updated.LatestReviewNote!.Comment);
    }

    [Fact]
    public async Task RequestChanges_EmptyComment_Returns400()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Empty Author {suffix}", $"empty-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Empty Comment {suffix}", PostStatus.PendingReview);

        var response = await editor.PostAsJsonAsync($"/api/posts/{post.Id}/request-changes",
            new RequestChangesRequest("   "));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RequestChanges_UnknownId_Returns404()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PostAsJsonAsync($"/api/posts/{Guid.NewGuid()}/request-changes",
            new RequestChangesRequest("Feedback"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RequestChanges_DraftPost_Returns409()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Draft2 Author {suffix}", $"draft2-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Still Draft 2 {suffix}", PostStatus.Draft);

        var response = await editor.PostAsJsonAsync($"/api/posts/{post.Id}/request-changes",
            new RequestChangesRequest("Feedback"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RequestChanges_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/posts/{Guid.NewGuid()}/request-changes",
            new RequestChangesRequest("Feedback"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestChanges_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PostAsJsonAsync($"/api/posts/{Guid.NewGuid()}/request-changes",
            new RequestChangesRequest("Feedback"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RequestChanges_LogsActivityEntry()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"RCLog Author {suffix}", $"rclog-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"RC Log Me {suffix}", PostStatus.PendingReview);

        var response = await editor.PostAsJsonAsync($"/api/posts/{post.Id}/request-changes",
            new RequestChangesRequest("Needs work."));
        response.EnsureSuccessStatusCode();

        var activityResponse = await owner.GetAsync("/api/activity");
        var entries = await activityResponse.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.Contains(entries!, e => e.Action == $"requested changes on \"RC Log Me {suffix}\"");
    }
}
