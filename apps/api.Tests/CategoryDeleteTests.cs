using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class CategoryDeleteTests(ApiFactory factory)
{
    [Fact]
    public async Task Delete_CategoryWithNoPosts_Returns204AndRemovesIt()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var created = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Delete Test {suffix}", $"delete-test-{suffix}");

        var response = await editor.DeleteAsync($"/api/categories/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var listResponse = await editor.GetAsync("/api/categories");
        var categories = await listResponse.Content.ReadFromJsonAsync<List<CategoryDto>>();
        Assert.DoesNotContain(categories!, c => c.Id == created.Id);
    }

    [Fact]
    public async Task Delete_CategoryWithReferencingPost_Returns409AndDoesNotRemoveIt()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var created = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Blocked Delete {suffix}", $"blocked-delete-{suffix}");
        await DbTestHelpers.AddPostReferencingCategoryAsync(factory, created.Id);

        var response = await editor.DeleteAsync($"/api/categories/{created.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var stillThere = await CategoryTestHelpers.GetCategoryAsync(editor, created.Id);
        Assert.Equal(created.Id, stillThere.Id);
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
