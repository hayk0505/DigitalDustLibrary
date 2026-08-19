using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PostTagsTests(ApiFactory factory)
{
    [Fact]
    public async Task Patch_SetsTagsAndReflectsThemInResponse()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var tagA = await TagTestHelpers.CreateTagAsync(author, $"Patch Tag A {suffix}");
        var tagB = await TagTestHelpers.CreateTagAsync(author, $"Patch Tag B {suffix}");
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Tag Patch Test {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await author.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, null, [tagA.Id, tagB.Id]));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(2, updated!.Tags.Count);
        Assert.Contains(updated.Tags, t => t.Id == tagA.Id);
        Assert.Contains(updated.Tags, t => t.Id == tagB.Id);
    }

    [Fact]
    public async Task Patch_ReplacingTags_RemovesThePreviousSet()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var tagA = await TagTestHelpers.CreateTagAsync(author, $"Replace Tag A {suffix}");
        var tagB = await TagTestHelpers.CreateTagAsync(author, $"Replace Tag B {suffix}");
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Tag Replace Test {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        var firstPatch = await author.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, null, [tagA.Id]));
        firstPatch.EnsureSuccessStatusCode();

        var response = await author.PatchAsJsonAsync($"/api/posts/{created.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, null, [tagB.Id]));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Single(updated!.Tags);
        Assert.Equal(tagB.Id, updated.Tags[0].Id);
    }

    [Fact]
    public async Task Patch_WithUnknownTagId_Returns400()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Bad Tag Patch {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await author.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, null, [Guid.NewGuid()]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithUnknownTagId_Returns400()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();

        var response = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Bad Tag Create {suffix}", null, null, null, null, null, null, null, [Guid.NewGuid()]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithoutTagIds_CreatesUntaggedPost()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();

        var response = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"No Tags {suffix}", null, null, null, null, null, null, null));

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Empty(created!.Tags);
    }
}
