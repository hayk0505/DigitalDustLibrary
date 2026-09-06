using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PostSourceTests(ApiFactory factory)
{
    [Fact]
    public async Task Create_WithSource_Persists()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();

        var response = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Sourced Post {suffix}", null, null, null, null, null, null, null,
                Source: "NASA press release"));

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal("NASA press release", created!.Source);
    }

    [Fact]
    public async Task Create_OmittingSource_DefaultsToNull()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();

        var response = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"No Source {suffix}", null, null, null, null, null, null, null));

        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Null(created!.Source);
    }

    [Fact]
    public async Task Patch_UpdatesSource()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Patch Source {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await author.PatchAsJsonAsync($"/api/posts/{created!.Id}",
            new UpdatePostRequest(null, null, null, null, null, null, null, null,
                Source: "Reuters, Sept 2026"));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);
        Assert.Equal("Reuters, Sept 2026", updated!.Source);
    }

    [Fact]
    public async Task PublicEndpoint_ExposesSource()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Source Author {suffix}", $"source-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(
            factory, author.Id, $"Public Sourced Post {suffix}", PostStatus.PendingReview,
            source: "U.S. Census Bureau");
        await editor.PostAsync($"/api/posts/{post.Id}/approve", null);

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/public/posts");

        response.EnsureSuccessStatusCode();
        var posts = await response.Content.ReadFromJsonAsync<List<PublicPostDto>>(AuthHelper.JsonOptions);
        var publicPost = Assert.Single(posts!, p => p.Title == $"Public Sourced Post {suffix}");
        Assert.Equal("U.S. Census Bureau", publicPost.Source);
    }
}
