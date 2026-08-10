using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class UserPatchTests(ApiFactory factory)
{
    [Fact]
    public async Task Patch_ChangesRole()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = UserTestHelpers.UniqueSuffix();
        var target = await UserTestHelpers.CreateUserAsync(
            factory, $"Role Test {suffix}", $"role-{suffix}@example.com", Role.Author);

        var response = await owner.PatchAsJsonAsync($"/api/users/{target.Id}", new UpdateUserRequest(Role.Editor, null));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<ManagedUserDto>(AuthHelper.JsonOptions);
        Assert.Equal(Role.Editor, updated!.Role);
    }

    [Fact]
    public async Task Patch_DeactivatesUser()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = UserTestHelpers.UniqueSuffix();
        var target = await UserTestHelpers.CreateUserAsync(
            factory, $"Deactivate Test {suffix}", $"deactivate-{suffix}@example.com", Role.Author);

        var response = await owner.PatchAsJsonAsync($"/api/users/{target.Id}", new UpdateUserRequest(null, false));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<ManagedUserDto>(AuthHelper.JsonOptions);
        Assert.False(updated!.IsActive);
    }

    [Fact]
    public async Task Patch_DeactivateUser_RevokesActiveRefreshTokens()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = UserTestHelpers.UniqueSuffix();
        var target = await UserTestHelpers.CreateUserAsync(
            factory, $"Revoke Test {suffix}", $"revoke-{suffix}@example.com", Role.Author);

        // Log in as the target to get a real refresh cookie, extracted manually
        // from Set-Cookie since WebApplicationFactory's HttpClient doesn't
        // auto-persist cookies across separate requests.
        var targetClient = factory.CreateClient();
        var loginResponse = await targetClient.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(target.Email, UserTestHelpers.Password));
        loginResponse.EnsureSuccessStatusCode();
        var refreshCookie = loginResponse.Headers.GetValues("Set-Cookie")
            .Single(h => h.StartsWith("refreshToken=")).Split(';')[0];

        var patchResponse = await owner.PatchAsJsonAsync($"/api/users/{target.Id}", new UpdateUserRequest(null, false));
        patchResponse.EnsureSuccessStatusCode();

        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refreshRequest.Headers.Add("Cookie", refreshCookie);
        var refreshResponse = await targetClient.SendAsync(refreshRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Patch_UnknownId_Returns404()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);

        var response = await owner.PatchAsJsonAsync($"/api/users/{Guid.NewGuid()}", new UpdateUserRequest(Role.Editor, null));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Patch_OwnerDemotesSelf_Returns409()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ownerUser = await db.Users.SingleAsync(u => u.Email == AuthHelper.OwnerEmail);

        var response = await owner.PatchAsJsonAsync($"/api/users/{ownerUser.Id}", new UpdateUserRequest(Role.Editor, null));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Patch_OwnerDeactivatesSelf_Returns409()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ownerUser = await db.Users.SingleAsync(u => u.Email == AuthHelper.OwnerEmail);

        var response = await owner.PatchAsJsonAsync($"/api/users/{ownerUser.Id}", new UpdateUserRequest(null, false));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Patch_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync($"/api/users/{Guid.NewGuid()}", new UpdateUserRequest(Role.Editor, null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Patch_AsEditor_Returns403()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PatchAsJsonAsync($"/api/users/{Guid.NewGuid()}", new UpdateUserRequest(Role.Editor, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Patch_ChangesRole_LogsActivityEntry()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = UserTestHelpers.UniqueSuffix();
        var target = await UserTestHelpers.CreateUserAsync(
            factory, $"Role Log {suffix}", $"role-log-{suffix}@example.com", Role.Author);

        var response = await owner.PatchAsJsonAsync($"/api/users/{target.Id}", new UpdateUserRequest(Role.Editor, null));
        response.EnsureSuccessStatusCode();

        var activityResponse = await owner.GetAsync("/api/activity");
        var entries = await activityResponse.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.Contains(entries!, e => e.Action == $"changed Role Log {suffix}'s role to Editor");
    }

    [Fact]
    public async Task Patch_DeactivatesUser_LogsActivityEntry()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = UserTestHelpers.UniqueSuffix();
        var target = await UserTestHelpers.CreateUserAsync(
            factory, $"Deactivate Log {suffix}", $"deactivate-log-{suffix}@example.com", Role.Author);

        var response = await owner.PatchAsJsonAsync($"/api/users/{target.Id}", new UpdateUserRequest(null, false));
        response.EnsureSuccessStatusCode();

        var activityResponse = await owner.GetAsync("/api/activity");
        var entries = await activityResponse.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.Contains(entries!, e => e.Action == $"deactivated Deactivate Log {suffix}");
    }
}
