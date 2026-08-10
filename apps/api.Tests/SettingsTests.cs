using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class SettingsTests(ApiFactory factory)
{
    [Fact]
    public async Task Get_AsOwner_ReturnsOkWithSettingsShape()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);

        var response = await owner.GetAsync("/api/settings");

        response.EnsureSuccessStatusCode();
        var settings = await response.Content.ReadFromJsonAsync<SiteSettingsDto>(AuthHelper.JsonOptions);
        Assert.NotEqual(Guid.Empty, settings!.Id);
    }

    [Fact]
    public async Task Get_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/settings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_AsEditor_Returns403()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.GetAsync("/api/settings");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.GetAsync("/api/settings");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Patch_AsOwner_UpdatesAllFieldsAndPersists()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = Guid.NewGuid().ToString("N")[..12];

        var patchResponse = await owner.PatchAsJsonAsync("/api/settings",
            new UpdateSiteSettingsRequest(
                $"Title {suffix}", $"Tagline {suffix}", $"Meta {suffix}",
                $"https://linkedin.com/{suffix}", $"https://x.com/{suffix}"));

        patchResponse.EnsureSuccessStatusCode();
        var updated = await patchResponse.Content.ReadFromJsonAsync<SiteSettingsDto>(AuthHelper.JsonOptions);
        Assert.Equal($"Title {suffix}", updated!.SiteTitle);
        Assert.Equal($"https://x.com/{suffix}", updated.XUrl);

        var getResponse = await owner.GetAsync("/api/settings");
        var fetched = await getResponse.Content.ReadFromJsonAsync<SiteSettingsDto>(AuthHelper.JsonOptions);
        Assert.Equal($"Title {suffix}", fetched!.SiteTitle);
        Assert.Equal($"Tagline {suffix}", fetched.Tagline);
    }

    [Fact]
    public async Task Patch_WithEmptyLinkedInUrl_ClearsAPreviouslySetValue()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = Guid.NewGuid().ToString("N")[..12];

        var setResponse = await owner.PatchAsJsonAsync("/api/settings",
            new UpdateSiteSettingsRequest($"Title {suffix}", "Tagline", "Meta", "https://linkedin.com/set", "https://x.com/set"));
        setResponse.EnsureSuccessStatusCode();
        var withValue = await setResponse.Content.ReadFromJsonAsync<SiteSettingsDto>(AuthHelper.JsonOptions);
        Assert.Equal("https://linkedin.com/set", withValue!.LinkedInUrl);

        var clearResponse = await owner.PatchAsJsonAsync("/api/settings",
            new UpdateSiteSettingsRequest($"Title {suffix}", "Tagline", "Meta", "", "https://x.com/set"));

        clearResponse.EnsureSuccessStatusCode();
        var cleared = await clearResponse.Content.ReadFromJsonAsync<SiteSettingsDto>(AuthHelper.JsonOptions);
        Assert.Equal("", cleared!.LinkedInUrl);
    }

    [Fact]
    public async Task Patch_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync("/api/settings",
            new UpdateSiteSettingsRequest("No Auth", "", "", "", ""));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Patch_AsEditor_Returns403()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PatchAsJsonAsync("/api/settings",
            new UpdateSiteSettingsRequest("Editor Attempt", "", "", "", ""));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
