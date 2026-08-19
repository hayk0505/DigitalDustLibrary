using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

// AudioTrackScannerTests covers the filename-parsing logic in isolation; this
// covers the endpoint's own wiring — that it scans the real wwwroot/audio
// directory the app is configured with, and that the URLs it returns are
// actually servable, not just well-formed strings. Writes straight into that
// directory (there's no upload endpoint for audio — see AudioTrackScanner's
// doc comment, tracks are scp'd onto the droplet in prod) and always cleans
// up, since anything left behind would keep showing up in every future local
// `dotnet run` against this same wwwroot until someone noticed and deleted it
// by hand.
[Collection(ApiCollection.Name)]
public class PublicAudioEndpointTests(ApiFactory factory)
{
    private static string AudioDir(ApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        return Path.Combine(env.ContentRootPath, "wwwroot", "audio");
    }

    [Fact]
    public async Task GetAudio_ReturnsScannedTrackWithAbsoluteServableUrl()
    {
        var audioDir = AudioDir(factory);
        Directory.CreateDirectory(audioDir);
        var suffix = TagTestHelpers.UniqueSuffix();
        var filename = $"Test Artist {suffix} - Test Track {suffix}.mp3";
        var filePath = Path.Combine(audioDir, filename);
        var bytes = new byte[] { 1, 2, 3, 4 };
        await File.WriteAllBytesAsync(filePath, bytes);

        try
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("/api/public/audio");
            response.EnsureSuccessStatusCode();

            var tracks = await response.Content.ReadFromJsonAsync<List<AudioTrack>>(AuthHelper.JsonOptions);
            var found = tracks!.Single(t => t.Title == $"Test Track {suffix}");
            Assert.Equal($"Test Artist {suffix}", found.Artist);

            // Built from ApiPublicOrigin (localhost:5080 in the Development
            // config ApiFactory forces), not a relative path — the blog calls
            // this cross-origin, so a relative URL would resolve against the
            // blog's own origin instead of the API's.
            Assert.StartsWith("http://localhost:5080/audio/", found.Src);

            var fileResponse = await client.GetAsync(found.Src);
            Assert.Equal(HttpStatusCode.OK, fileResponse.StatusCode);
            Assert.Equal(bytes, await fileResponse.Content.ReadAsByteArrayAsync());
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task GetAudio_IgnoresFilesWithUnsupportedExtensions()
    {
        var audioDir = AudioDir(factory);
        Directory.CreateDirectory(audioDir);
        var suffix = TagTestHelpers.UniqueSuffix();
        var filename = $"Ignore Me {suffix}.txt";
        var filePath = Path.Combine(audioDir, filename);
        await File.WriteAllTextAsync(filePath, "not audio");

        try
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("/api/public/audio");
            response.EnsureSuccessStatusCode();

            var tracks = await response.Content.ReadFromJsonAsync<List<AudioTrack>>(AuthHelper.JsonOptions);
            Assert.DoesNotContain(tracks!, t => t.Title.Contains(suffix));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task GetAudio_DoesNotRequireAuth()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/public/audio");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
