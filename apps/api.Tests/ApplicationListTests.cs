using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class ApplicationListTests(ApiFactory factory)
{
    [Fact]
    public async Task Get_ReturnsApplicationsNewestFirst()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var older = await ApplicationTestHelpers.CreatePendingApplicationAsync(
            factory, $"Older {suffix}", $"older-{suffix}@example.com", "Pitch A");
        await Task.Delay(10);
        var newer = await ApplicationTestHelpers.CreatePendingApplicationAsync(
            factory, $"Newer {suffix}", $"newer-{suffix}@example.com", "Pitch B");

        var response = await editor.GetAsync("/api/applications");
        response.EnsureSuccessStatusCode();
        var applications = await response.Content.ReadFromJsonAsync<List<AuthorApplicationDto>>(AuthHelper.JsonOptions);

        var indexOfNewer = applications!.FindIndex(a => a.Id == newer.Id);
        var indexOfOlder = applications.FindIndex(a => a.Id == older.Id);
        Assert.True(indexOfNewer >= 0, "expected the newer application to be in the list");
        Assert.True(indexOfOlder >= 0, "expected the older application to be in the list");
        Assert.True(indexOfNewer < indexOfOlder, "expected the newer application to sort first");
    }

    [Fact]
    public async Task Get_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/applications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.GetAsync("/api/applications");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
