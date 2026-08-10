using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class ActivityLogTests(ApiFactory factory)
{
    [Fact]
    public async Task Get_AsOwner_ReturnsOkWithActivityShape()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);

        var response = await owner.GetAsync("/api/activity");

        response.EnsureSuccessStatusCode();
        var entries = await response.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.NotNull(entries);
    }

    [Fact]
    public async Task Get_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/activity");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_AsEditor_Returns403()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.GetAsync("/api/activity");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.GetAsync("/api/activity");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_MineTrue_AsEditor_ReturnsOwnEntry()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var created = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Mine Hide {suffix}", $"mine-hide-{suffix}");
        await editor.PatchAsJsonAsync($"/api/categories/{created.Id}", new UpdateCategoryRequest(null, null, false, null));

        var response = await editor.GetAsync("/api/activity?mine=true");

        response.EnsureSuccessStatusCode();
        var entries = await response.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.Contains(entries!, e => e.Action == $"hid \"Mine Hide {suffix}\"");
    }

    [Fact]
    public async Task Get_MineTrue_AsAuthor_ExcludesOtherActorsEntries()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var created = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Exclude Test {suffix}", $"exclude-test-{suffix}");
        await editor.PatchAsJsonAsync($"/api/categories/{created.Id}", new UpdateCategoryRequest(null, null, false, null));

        var response = await author.GetAsync("/api/activity?mine=true");

        response.EnsureSuccessStatusCode();
        var entries = await response.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.DoesNotContain(entries!, e => e.Action == $"hid \"Exclude Test {suffix}\"");
    }
}
