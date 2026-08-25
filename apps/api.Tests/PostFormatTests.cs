using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PostFormatTests(ApiFactory factory)
{
    [Fact]
    public async Task Create_OmittingFormat_DefaultsToShelf()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();

        var response = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"No Format {suffix}", null, null, null, null, null, null, null));

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(PostFormat.Shelf, created!.Format);
    }

    [Fact]
    public async Task Create_WithExplicitMiniFormat_Persists()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();

        var response = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Explicit Mini {suffix}", null, null, null, null, null, null, null, Format: PostFormat.Mini));

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(PostFormat.Mini, created!.Format);
    }

    [Fact]
    public async Task Patch_UpdatesFormat()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Patch Format {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await author.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, null, Format: PostFormat.Mini));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal(PostFormat.Mini, updated!.Format);
    }
}
