using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class AuthMeTests(ApiFactory factory)
{
    [Fact]
    public async Task Get_WithValidToken_ReturnsCurrentUser()
    {
        var client = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await client.GetAsync("/api/auth/me");

        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<UserDto>(AuthHelper.JsonOptions);
        Assert.Equal(AuthHelper.EditorEmail, user!.Email);
    }

    [Fact]
    public async Task Get_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
