using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class TagCreateTests(ApiFactory factory)
{
    [Fact]
    public async Task Post_NewName_Returns201WithComputedSlug()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();

        var response = await author.PostAsJsonAsync("/api/tags", new CreateTagRequest($"Internet Culture {suffix}"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.Equal($"Internet Culture {suffix}", created!.Name);
        Assert.Equal($"internet-culture-{suffix}".ToLowerInvariant(), created.Slug);
        Assert.Equal(0, created.PostCount);
    }

    [Fact]
    public async Task Post_AsAuthor_Returns201NotForbidden()
    {
        // The one endpoint in this group open to Authors, unlike Category's
        // Editor/Owner-only POST — free-typing a new tag while drafting must
        // not require an Editor/Owner round-trip.
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PostAsJsonAsync("/api/tags", new CreateTagRequest($"Author Tag {TagTestHelpers.UniqueSuffix()}"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_SameNameTwice_ReturnsSameTagBothTimes()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var name = $"Repeat Tag {suffix}";

        var first = await TagTestHelpers.CreateTagAsync(author, name);
        var secondResponse = await author.PostAsJsonAsync("/api/tags", new CreateTagRequest(name));

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var second = await secondResponse.Content.ReadFromJsonAsync<TagDto>();
        Assert.Equal(first.Id, second!.Id);
    }

    [Fact]
    public async Task Post_NameDifferingOnlyByCase_ResolvesToSameTag()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var first = await TagTestHelpers.CreateTagAsync(author, $"AI {suffix}");

        var secondResponse = await author.PostAsJsonAsync("/api/tags", new CreateTagRequest($"ai {suffix}"));

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var second = await secondResponse.Content.ReadFromJsonAsync<TagDto>();
        Assert.Equal(first.Id, second!.Id);
    }

    [Fact]
    public async Task Post_BlankName_Returns400()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PostAsJsonAsync("/api/tags", new CreateTagRequest("   "));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tags", new CreateTagRequest("No Auth"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
