using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigitalDustLibrary.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DigitalDustLibrary.Api.Tests;

// Logs in via the real POST /api/auth/login (no mocking) using one of the
// three demo accounts DbSeeder.cs seeds on startup, all password "password".
public static class AuthHelper
{
    public const string EditorEmail = "editor@dd.local";
    public const string OwnerEmail = "owner@dd.local";
    public const string AuthorEmail = "author@dd.local";
    private const string Password = "password";

    // Mirrors Program.cs's ConfigureHttpJsonOptions: HttpClient's JSON helpers
    // don't know about the server's custom enum converter by default, so
    // UserDto.Role (serialized as "owner"/"editor"/"author") fails to
    // deserialize without this.
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
    };

    public static async Task<HttpClient> LoginAsAsync(WebApplicationFactory<Program> factory, string email)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, Password));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return client;
    }
}
