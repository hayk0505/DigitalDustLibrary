using System.Net;
using System.Net.Http.Json;
using System.Text;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Models;
using SkiaSharp;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class MediaUploadTests(ApiFactory factory)
{
    // A well-known 1x1 transparent PNG, used across the ecosystem as a minimal
    // valid test image — small enough to inline here as a literal.
    private const string OnePixelPngDataUrl =
        "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";

    // A well-known 1x1 transparent GIF, minimal valid literal (same rationale
    // as OnePixelPngDataUrl above).
    private const string OnePixelGifDataUrl =
        "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==";

    [Fact]
    public async Task Post_ValidPngDataUrl_TranscodesToWebpAndReturnsAbsoluteStaticUrl()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PostAsJsonAsync("/api/media",
            new CreateMediaRequest("pixel.png", OnePixelPngDataUrl, MediaTag.Inline, 1, 1), AuthHelper.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<MediaAssetDto>(AuthHelper.JsonOptions);

        // Must be a real static URL, not the data: URI echoed back — and
        // every raster upload gets transcoded to WebP regardless of the
        // format it arrived in.
        Assert.StartsWith("http://", created!.Url);
        Assert.EndsWith(".webp", created.Url);
        Assert.DoesNotContain("base64", created.Url);

        // The file must actually be servable, and decode as real WebP.
        using var anonymousClient = factory.CreateClient();
        var fileResponse = await anonymousClient.GetAsync(created.Url);
        Assert.Equal(HttpStatusCode.OK, fileResponse.StatusCode);
        var bytes = await fileResponse.Content.ReadAsByteArrayAsync();
        using var codec = SKCodec.Create(new SKMemoryStream(bytes));
        Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
        Assert.Equal(1, codec.Info.Width);
        Assert.Equal(1, codec.Info.Height);
    }

    [Fact]
    public async Task Post_OversizedDimensions_IsDownscaledToMax1200OnLongestEdge()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        using var source = new SKBitmap(2400, 1200);
        using var sourceImage = SKImage.FromBitmap(source);
        using var sourcePng = sourceImage.Encode(SKEncodedImageFormat.Png, 100);
        var dataUrl = $"data:image/png;base64,{Convert.ToBase64String(sourcePng.ToArray())}";

        var response = await editor.PostAsJsonAsync("/api/media",
            new CreateMediaRequest("wide.png", dataUrl, MediaTag.Inline, 2400, 1200), AuthHelper.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<MediaAssetDto>(AuthHelper.JsonOptions);

        // Server-computed dimensions must reflect the actual resize, not the
        // stale client-sent width/height (2400x1200).
        Assert.Equal(1200, created!.Width);
        Assert.Equal(600, created.Height);

        using var anonymousClient = factory.CreateClient();
        var bytes = await (await anonymousClient.GetAsync(created.Url)).Content.ReadAsByteArrayAsync();
        using var codec = SKCodec.Create(new SKMemoryStream(bytes));
        Assert.Equal(1200, codec.Info.Width);
        Assert.Equal(600, codec.Info.Height);
    }

    [Fact]
    public async Task Post_SvgDataUrl_SkipsTranscodingAndStoresAsIs()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        const string svg = "<svg xmlns='http://www.w3.org/2000/svg' width='10' height='10'><rect width='10' height='10'/></svg>";
        var svgBytes = Encoding.UTF8.GetBytes(svg);
        var dataUrl = $"data:image/svg+xml;base64,{Convert.ToBase64String(svgBytes)}";

        var response = await editor.PostAsJsonAsync("/api/media",
            new CreateMediaRequest("icon.svg", dataUrl, MediaTag.Inline, 10, 10), AuthHelper.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<MediaAssetDto>(AuthHelper.JsonOptions);

        // Vector images aren't rasterized — stored byte-for-byte as uploaded.
        Assert.EndsWith(".svg", created!.Url);
        using var anonymousClient = factory.CreateClient();
        var bytes = await (await anonymousClient.GetAsync(created.Url)).Content.ReadAsByteArrayAsync();
        Assert.Equal(svgBytes, bytes);
    }

    [Fact]
    public async Task Post_GifDataUrl_SkipsTranscodingAndStoresAsIs()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PostAsJsonAsync("/api/media",
            new CreateMediaRequest("pixel.gif", OnePixelGifDataUrl, MediaTag.Inline, 1, 1), AuthHelper.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<MediaAssetDto>(AuthHelper.JsonOptions);

        // GIFs are skipped so any animation survives — WebP re-encoding
        // would otherwise flatten it to a single frame.
        Assert.EndsWith(".gif", created!.Url);
        using var anonymousClient = factory.CreateClient();
        var bytes = await (await anonymousClient.GetAsync(created.Url)).Content.ReadAsByteArrayAsync();
        Assert.Equal(Convert.FromBase64String(OnePixelGifDataUrl.Split(',')[1]), bytes);
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
