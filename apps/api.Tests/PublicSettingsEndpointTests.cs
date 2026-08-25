using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PublicSettingsEndpointTests(ApiFactory factory)
{
    [Fact]
    public async Task GetSettings_DoesNotRequireAuth()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/public/settings");

        response.EnsureSuccessStatusCode();
        var settings = await response.Content.ReadFromJsonAsync<SiteSettingsDto>(AuthHelper.JsonOptions);
        Assert.NotEqual(Guid.Empty, settings!.Id);
    }

    [Fact]
    public async Task GetSettings_ReflectsLatestOwnerPatch()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var patchResponse = await owner.PatchAsJsonAsync("/api/settings",
            new UpdateSiteSettingsRequest($"Public Title {suffix}", $"Public Tagline {suffix}", "Meta", "", ""));
        patchResponse.EnsureSuccessStatusCode();

        var anonymousClient = factory.CreateClient();
        var response = await anonymousClient.GetAsync("/api/public/settings");

        response.EnsureSuccessStatusCode();
        var settings = await response.Content.ReadFromJsonAsync<SiteSettingsDto>(AuthHelper.JsonOptions);
        Assert.Equal($"Public Title {suffix}", settings!.SiteTitle);
        Assert.Equal($"Public Tagline {suffix}", settings.Tagline);
    }
}
