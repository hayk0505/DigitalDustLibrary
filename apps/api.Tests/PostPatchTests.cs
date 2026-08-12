using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PostPatchTests(ApiFactory factory)
{
    [Fact]
    public async Task Patch_AuthorEditsOwnDraft_Succeeds()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Draft {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await author.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest($"Edited {suffix}", null, null, null, null, null, null, null));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal($"Edited {suffix}", updated!.Title);
    }

    [Fact]
    public async Task Patch_AuthorSubmitsOwnDraftForReview_Succeeds()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Submit Test {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await author.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, PostStatus.PendingReview));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(PostStatus.PendingReview, updated!.Status);
    }

    [Fact]
    public async Task Patch_AnotherAuthorsPost_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var otherAuthor = await UserTestHelpers.CreateUserAsync(
            factory, $"Other Author {suffix}", $"other-author-{suffix}@example.com", Role.Author);
        var othersPost = await PostTestHelpers.CreatePostAsync(factory, otherAuthor.Id, $"Not Yours {suffix}");

        var response = await author.PatchAsJsonAsync($"/api/posts/{othersPost.Id}",
            new UpdatePostRequest("Hijacked Title", null, null, null, null, null, null, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Patch_SettingStatusToPublished_Returns400()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Bypass Test {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await author.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, PostStatus.Published));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Patch_SettingStatusToChangesRequested_Returns400()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Bypass Test 2 {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await author.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, PostStatus.ChangesRequested));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Patch_UnknownId_Returns404()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PatchAsJsonAsync($"/api/posts/{Guid.NewGuid()}",
            new UpdatePostRequest("Doesn't Matter", null, null, null, null, null, null, null));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Patch_OwnerSetsOwnPostToPublished_SucceedsAndSetsPublishedAt()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await owner.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Owner Publish {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await owner.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, PostStatus.Published));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(PostStatus.Published, updated!.Status);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbPost = await db.Posts.SingleAsync(p => p.Id == created.Id);
        Assert.NotNull(dbPost.PublishedAt);
    }

    [Fact]
    public async Task Patch_EditorSetsOwnPostToPublished_Succeeds()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await editor.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Editor Publish {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await editor.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, PostStatus.Published));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(PostStatus.Published, updated!.Status);
    }

    [Fact]
    public async Task Patch_OwnerSettingStatusToChangesRequested_StillReturns400()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await owner.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Owner ChangesRequested Blocked {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await owner.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, PostStatus.ChangesRequested));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Patch_TitleChangeBeforePublish_RegeneratesSlug()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Original Title {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        var originalSlug = created!.Slug;

        var response = await author.PatchAsJsonAsync($"/api/posts/{created.Id}",
            new UpdatePostRequest($"Renamed Title {suffix}", null, null, null, null, null, null, null));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.NotEqual(originalSlug, updated!.Slug);
        Assert.StartsWith("renamed-title", updated.Slug);
    }

    [Fact]
    public async Task Patch_TitleChangeAfterPublish_SlugStaysLocked()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await owner.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Lock Test {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var publishResponse = await owner.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, PostStatus.Published));
        publishResponse.EnsureSuccessStatusCode();
        var published = await publishResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        var lockedSlug = published!.Slug;

        var renameResponse = await owner.PatchAsJsonAsync($"/api/posts/{created.Id}",
            new UpdatePostRequest($"Renamed After Publish {suffix}", null, null, null, null, null, null, null));

        renameResponse.EnsureSuccessStatusCode();
        var renamed = await renameResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(lockedSlug, renamed!.Slug);
    }

    [Fact]
    public async Task Post_WithoutCategoryId_DefaultsToLowestPositionCategory()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        // Positioned below whatever the current global minimum is (not a fixed
        // constant) so this assertion holds regardless of what other tests have
        // already created in the shared, never-reset test database and
        // regardless of test execution order.
        var floor = await CategoryTestHelpers.LowestExistingPositionAsync(editor);
        var lowestCategory = await CategoryTestHelpers.CreateCategoryAsync(
            editor, $"Lowest Position {suffix}", $"lowest-position-{suffix}", position: floor - 1000);

        var response = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"No Category Given {suffix}", null, null, null, null, null, null, null));

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(lowestCategory.Id, created!.CategoryId);
    }

    [Fact]
    public async Task Post_WithoutCategoryId_SkipsDeletedLowestPositionCategory()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        // Both positions are anchored below the current global minimum (see
        // comment above) so nextLowest is guaranteed to be the true minimum
        // among non-deleted categories, and deletedLowest would have been the
        // global minimum were it not soft-deleted — independent of test order.
        var floor = await CategoryTestHelpers.LowestExistingPositionAsync(editor);
        var deletedLowest = await CategoryTestHelpers.CreateCategoryAsync(
            editor, $"Deleted Lowest {suffix}", $"deleted-lowest-{suffix}", position: floor - 2000);
        await editor.PatchAsJsonAsync($"/api/categories/{deletedLowest.Id}", new UpdateCategoryRequest(null, null, null, true));
        var nextLowest = await CategoryTestHelpers.CreateCategoryAsync(
            editor, $"Next Lowest {suffix}", $"next-lowest-{suffix}", position: floor - 1000);

        var response = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Skip Deleted {suffix}", null, null, null, null, null, null, null));

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(nextLowest.Id, created!.CategoryId);
    }

    [Fact]
    public async Task Post_WithNonexistentCategoryId_Returns400NotServerError()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();

        var response = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Bad Category {suffix}", null, null, null, null, null, Guid.NewGuid(), null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Patch_WithNonexistentCategoryId_Returns400NotServerError()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Patch Bad Category {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await author.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, Guid.NewGuid(), null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
