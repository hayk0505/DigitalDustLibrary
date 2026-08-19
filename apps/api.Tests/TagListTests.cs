using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class TagListTests(ApiFactory factory)
{
    [Fact]
    public async Task Get_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tags");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_AsAuthor_Returns200()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.GetAsync("/api/tags");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Get_ReturnsTagsOrderedByName()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var zzz = await TagTestHelpers.CreateTagAsync(author, $"ZZZ Last {suffix}");
        var aaa = await TagTestHelpers.CreateTagAsync(author, $"AAA First {suffix}");

        var response = await author.GetAsync("/api/tags");
        response.EnsureSuccessStatusCode();
        var tags = await response.Content.ReadFromJsonAsync<List<TagDto>>();

        var indexOfAaa = tags!.FindIndex(t => t.Id == aaa.Id);
        var indexOfZzz = tags.FindIndex(t => t.Id == zzz.Id);
        Assert.True(indexOfAaa >= 0 && indexOfZzz >= 0, "expected both newly created tags to be in the list");
        Assert.True(indexOfAaa < indexOfZzz, "expected alphabetical ordering by Name");
    }
}
