using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class CategoryListTests(ApiFactory factory)
{
    [Fact]
    public async Task Get_ReturnsCategoriesOrderedByPosition()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        // Deliberately named/created so name order and creation order both
        // disagree with the Position this test actually asserts on.
        var first = await CategoryTestHelpers.CreateCategoryAsync(
            editor, $"ZZZ First {suffix}", $"zzz-first-{suffix}", position: 1000);
        var second = await CategoryTestHelpers.CreateCategoryAsync(
            editor, $"AAA Second {suffix}", $"aaa-second-{suffix}", position: 1001);

        var response = await editor.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();

        var indexOfFirst = categories!.FindIndex(c => c.Id == first.Id);
        var indexOfSecond = categories.FindIndex(c => c.Id == second.Id);
        Assert.True(indexOfFirst >= 0 && indexOfSecond >= 0, "expected both newly created categories to be in the list");
        Assert.True(indexOfFirst < indexOfSecond, "expected the lower-Position category to sort first despite its higher name/later creation");
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
    public async Task Get_AsAuthor_Returns200()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.GetAsync("/api/categories");

        response.EnsureSuccessStatusCode();
    }
}
