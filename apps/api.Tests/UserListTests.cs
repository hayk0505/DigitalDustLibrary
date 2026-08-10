using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class UserListTests(ApiFactory factory)
{
    [Fact]
    public async Task Get_ReturnsSeededAccountsWithCorrectShape()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);

        var response = await owner.GetAsync("/api/users");

        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<ManagedUserDto>>(AuthHelper.JsonOptions);
        var ownerEntry = users!.Single(u => u.Email == AuthHelper.OwnerEmail);
        Assert.Equal(Role.Owner, ownerEntry.Role);
        Assert.True(ownerEntry.IsActive);
        Assert.Contains(users, u => u.Email == AuthHelper.EditorEmail);
        Assert.Contains(users, u => u.Email == AuthHelper.AuthorEmail);
    }

    [Fact]
    public async Task Get_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_AsEditor_Returns403()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
