using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class TagUpdateTests(ApiFactory factory)
{
    [Fact]
    public async Task Patch_RenamesTag()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var created = await TagTestHelpers.CreateTagAsync(editor, $"Old Name {suffix}");

        var response = await editor.PatchAsJsonAsync($"/api/tags/{created.Id}",
            new UpdateTagRequest($"New Name {suffix}", $"new-slug-{suffix}"));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.Equal($"New Name {suffix}", updated!.Name);
        Assert.Equal($"new-slug-{suffix}", updated.Slug);
    }

    [Fact]
    public async Task Patch_SlugConflictingWithAnotherTag_Returns409AndLeavesTargetUnchanged()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var taken = await TagTestHelpers.CreateTagAsync(editor, $"Taken {suffix}");
        var target = await TagTestHelpers.CreateTagAsync(editor, $"Target {suffix}");

        var response = await editor.PatchAsJsonAsync($"/api/tags/{target.Id}",
            new UpdateTagRequest(null, taken.Slug));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Patch_UnknownId_Returns404()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PatchAsJsonAsync($"/api/tags/{Guid.NewGuid()}",
            new UpdateTagRequest("Doesn't Matter", null));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Patch_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PatchAsJsonAsync($"/api/tags/{Guid.NewGuid()}",
            new UpdateTagRequest("Author Attempt", null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Patch_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync($"/api/tags/{Guid.NewGuid()}", new UpdateTagRequest("No Auth", null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
