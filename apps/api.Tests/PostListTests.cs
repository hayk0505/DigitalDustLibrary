using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PostListTests(ApiFactory factory)
{
    [Fact]
    public async Task Get_ReturnsAuthorName()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"AuthorName Test {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var listResponse = await author.GetAsync("/api/posts?mine=true");
        listResponse.EnsureSuccessStatusCode();
        var posts = await listResponse.Content.ReadFromJsonAsync<List<PostDto>>(AuthHelper.JsonOptions);

        var found = posts!.Single(p => p.Id == created!.Id);
        // "Alex Rivera" is the seeded name for author@dd.local — see Data/DbSeeder.cs.
        Assert.Equal("Alex Rivera", found.AuthorName);
    }

    // Regression test: an Editor/Owner creating a brand-new post directly
    // with Status: Published (skipping Draft -> PendingReview -> approve
    // entirely) used to leave PublishedAt null, since only PATCH and
    // /approve set it — POST / never did. A Published post with a null
    // PublishedAt is invisible on the public blog (see
    // PublicEndpointsTests.GetPosts_PublishedPostWithNullPublishedAt_IsExcludedNotThrown),
    // so this silently vanished the post the moment it was created.
    [Fact]
    public async Task Post_CreatedDirectlyAsPublished_SetsPublishedAtAndIsPublic()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await editor.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Direct Publish {suffix}", null, null, null, null, null, null, PostStatus.Published));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        Assert.NotNull(created!.PublishedAt);

        var client = factory.CreateClient();
        var publicResponse = await client.GetAsync("/api/public/posts");
        publicResponse.EnsureSuccessStatusCode();
        var publicPosts = await publicResponse.Content.ReadFromJsonAsync<List<PublicPostDto>>(AuthHelper.JsonOptions);
        Assert.Contains(publicPosts!, p => p.Title == $"Direct Publish {suffix}");
    }
}
