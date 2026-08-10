using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class CategoryListTests(ApiFactory factory)
{
    [Fact]
    public async Task Get_ReturnsCategoriesOrderedByName()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var nameA = $"AAA Ordering {suffix}";
        var nameZ = $"ZZZ Ordering {suffix}";
        await CategoryTestHelpers.CreateCategoryAsync(editor, nameA, $"aaa-ordering-{suffix}");
        await CategoryTestHelpers.CreateCategoryAsync(editor, nameZ, $"zzz-ordering-{suffix}");

        var response = await editor.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();

        var indexOfA = categories!.FindIndex(c => c.Name == nameA);
        var indexOfZ = categories.FindIndex(c => c.Name == nameZ);
        Assert.True(indexOfA >= 0, "expected the newly created A-named category to be in the list");
        Assert.True(indexOfZ >= 0, "expected the newly created Z-named category to be in the list");
        Assert.True(indexOfA < indexOfZ, "expected the A-named category to sort before the Z-named category");
    }

    [Fact]
    public async Task Get_ReturnsZeroPostCountForCategoryWithNoPosts()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var created = await CategoryTestHelpers.CreateCategoryAsync(editor, $"PostCount Test {suffix}", $"postcount-{suffix}");

        var response = await editor.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();

        var found = categories!.Single(c => c.Id == created.Id);
        Assert.Equal(0, found.PostCount);
    }

    [Fact]
    public async Task Get_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
