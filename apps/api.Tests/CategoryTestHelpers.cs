using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

public static class CategoryTestHelpers
{
    public static async Task<CategoryDto> CreateCategoryAsync(
        HttpClient client, string name, string slug,
        string description = "Test category description.", string color = "#A27B5B", int? position = null)
    {
        var response = await client.PostAsJsonAsync(
            "/api/categories", new CreateCategoryRequest(name, slug, description, color, position));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryDto>())!;
    }

    // No GET /api/categories/{id} route exists — fetch the list and find it.
    public static async Task<CategoryDto> GetCategoryAsync(HttpClient client, Guid id)
    {
        var response = await client.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        return categories!.Single(c => c.Id == id);
    }

    public static string UniqueSuffix() => Guid.NewGuid().ToString("N")[..12];

    // Lets a test anchor a new category strictly below every category that
    // already exists (deleted or not) at call time, instead of asserting
    // against a fixed constant — the test DB is shared and never reset
    // between tests in this suite, so a fixed "-1000" can collide with
    // another test's data depending on run order.
    public static async Task<int> LowestExistingPositionAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        return categories!.Count > 0 ? categories.Min(c => c.Position) : 0;
    }
}
