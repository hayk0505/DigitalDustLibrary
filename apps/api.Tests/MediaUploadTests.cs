using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class MediaUploadTests(ApiFactory factory)
{
    // A well-known 1x1 transparent PNG, used across the ecosystem as a minimal
    // valid test image — small enough to inline here as a literal.
    private const string OnePixelPngDataUrl =
        "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";

    [Fact]
    public async Task Post_ValidPngDataUrl_WritesRealFileAndReturnsAbsoluteStaticUrl()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PostAsJsonAsync("/api/media",
            new CreateMediaRequest("pixel.png", OnePixelPngDataUrl, MediaTag.Inline, 1, 1), AuthHelper.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<MediaAssetDto>(AuthHelper.JsonOptions);

        // Must be a real static URL, not the data: URI echoed back.
        Assert.StartsWith("http://", created!.Url);
        Assert.EndsWith(".png", created.Url);
        Assert.DoesNotContain("base64", created.Url);

        // The file must actually be servable, with the right bytes.
        using var anonymousClient = factory.CreateClient();
        var fileResponse = await anonymousClient.GetAsync(created.Url);
        Assert.Equal(HttpStatusCode.OK, fileResponse.StatusCode);
        var bytes = await fileResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(Convert.FromBase64String(OnePixelPngDataUrl.Split(',')[1]), bytes);
    }

    [Fact]
    public async Task Post_MalformedDataUrl_Returns400()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PostAsJsonAsync("/api/media",
            new CreateMediaRequest("not-an-image.png", "not-a-data-url", MediaTag.Inline, 1, 1), AuthHelper.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_UnsupportedMimeType_Returns400()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PostAsJsonAsync("/api/media",
            new CreateMediaRequest("doc.pdf", "data:application/pdf;base64,JVBERi0xLjQK", MediaTag.Inline, 1, 1),
            AuthHelper.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_OversizedImage_Returns400()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        // All-'A' base64 decodes to zero bytes but is valid base64 (length is a
        // multiple of 4), so this exercises the size check specifically, not the
        // base64-format check.
        var oversizedDataUrl = "data:image/png;base64," + new string('A', 12_000_000);

        var response = await editor.PostAsJsonAsync("/api/media",
            new CreateMediaRequest("huge.png", oversizedDataUrl, MediaTag.Inline, 1, 1), AuthHelper.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/media",
            new CreateMediaRequest("pixel.png", OnePixelPngDataUrl, MediaTag.Inline, 1, 1), AuthHelper.JsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
