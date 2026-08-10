using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

public static class CategoryTestHelpers
{
    public static async Task<CategoryDto> CreateCategoryAsync(
        HttpClient client, string name, string slug, bool isPillar = false)
    {
        var response = await client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(name, slug, isPillar));
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
}
