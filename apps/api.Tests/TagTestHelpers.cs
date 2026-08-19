using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

public static class TagTestHelpers
{
    public static async Task<TagDto> CreateTagAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/tags", new CreateTagRequest(name));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TagDto>())!;
    }

    public static string UniqueSuffix() => Guid.NewGuid().ToString("N")[..12];
}
