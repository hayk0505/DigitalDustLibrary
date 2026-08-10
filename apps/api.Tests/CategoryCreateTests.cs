using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class CategoryCreateTests(ApiFactory factory)
{
    [Fact]
    public async Task Post_ValidRequest_Returns201WithDefaults()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();

        var response = await editor.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest($"Create Test {suffix}", $"create-test-{suffix}", false));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.Equal($"Create Test {suffix}", created!.Name);
        Assert.Equal($"create-test-{suffix}", created.Slug);
        Assert.Equal(0, created.PostCount);
        Assert.False(created.IsDeleted);
        Assert.True(created.IsVisible);
    }

    [Fact]
    public async Task Post_DuplicateSlug_Returns409()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var slug = $"dup-test-{suffix}";
        await CategoryTestHelpers.CreateCategoryAsync(editor, $"Dup Test {suffix}", slug);

        var response = await editor.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest($"Dup Test Again {suffix}", slug, false));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest("No Auth", $"no-auth-{CategoryTestHelpers.UniqueSuffix()}", false));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest("Author Attempt", $"author-attempt-{CategoryTestHelpers.UniqueSuffix()}", false));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
