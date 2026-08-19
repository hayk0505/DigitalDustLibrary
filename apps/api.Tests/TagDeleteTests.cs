using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class TagDeleteTests(ApiFactory factory)
{
    [Fact]
    public async Task Delete_UnusedTag_Returns204AndRemovesIt()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var created = await TagTestHelpers.CreateTagAsync(editor, $"Delete Test {suffix}");

        var response = await editor.DeleteAsync($"/api/tags/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var listResponse = await editor.GetAsync("/api/tags");
        var tags = await listResponse.Content.ReadFromJsonAsync<List<TagDto>>();
        Assert.DoesNotContain(tags!, t => t.Id == created.Id);
    }

    [Fact]
    public async Task Delete_TagReferencedByAPost_StillSucceeds()
    {
        // Unlike Category's hard-delete guard, Tag deletion is never blocked
        // by post count — untagging isn't destructive to the post itself.
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Tag Delete Author {TagTestHelpers.UniqueSuffix()}", $"tag-delete-author-{TagTestHelpers.UniqueSuffix()}@example.com", Role.Author);
        var suffix = TagTestHelpers.UniqueSuffix();
        var tag = await TagTestHelpers.CreateTagAsync(editor, $"Referenced {suffix}");
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Tagged Post {suffix}");
        await DbTestHelpers.AddPostTagAsync(factory, post.Id, tag.Id);

        var response = await editor.DeleteAsync($"/api/tags/{tag.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.DeleteAsync($"/api/tags/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.DeleteAsync($"/api/tags/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/tags/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
