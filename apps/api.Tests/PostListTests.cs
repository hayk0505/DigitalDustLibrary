using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

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
}
