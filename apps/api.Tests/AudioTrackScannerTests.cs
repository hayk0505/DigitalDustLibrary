using DigitalDustLibrary.Api.Services;

namespace DigitalDustLibrary.Api.Tests;

public class AudioTrackScannerTests
{
    // Each test gets its own throwaway directory under the OS temp folder —
    // Scan() only ever reads filenames, so empty files are enough, and a
    // fresh directory per call keeps tests from seeing each other's fixtures.
    private static List<AudioTrack> ScanFiles(params string[] filenames)
    {
        var dir = Path.Combine(Path.GetTempPath(), "audio-scanner-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            foreach (var name in filenames) File.WriteAllText(Path.Combine(dir, name), "");
            return AudioTrackScanner.Scan(dir);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Scan_NonexistentDirectory_ReturnsEmptyList()
    {
        var result = AudioTrackScanner.Scan(Path.Combine(Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid()));

        Assert.Empty(result);
    }

    [Fact]
    public void Scan_FiltersOutUnsupportedExtensions()
    {
        var result = ScanFiles("Artist - Song.mp3", "Artist - Notes.txt", "Artist - Cover.png");

        Assert.Single(result);
        Assert.Equal("Song", result[0].Title);
    }

    [Theory]
    [InlineData("mp3")]
    [InlineData("M4A")]
    [InlineData("aac")]
    [InlineData("ogg")]
    [InlineData("oga")]
    [InlineData("opus")]
    [InlineData("wav")]
    [InlineData("FLAC")]
    [InlineData("webm")]
    public void Scan_AcceptsAllSupportedExtensionsCaseInsensitively(string ext)
    {
        var result = ScanFiles($"Artist - Song.{ext}");

        Assert.Single(result);
    }

    [Theory]
    [InlineData("Artist - Title.mp3", "Artist", "Title")]
    [InlineData("  Padded Artist   -   Padded Title  .mp3", "Padded Artist", "Padded Title")]
    [InlineData("A - B - C.mp3", "A", "B - C")]
    public void Scan_ParsesArtistAndTitleAtFirstSeparator(string filename, string artist, string title)
    {
        var result = ScanFiles(filename);

        Assert.Equal(artist, result[0].Artist);
        Assert.Equal(title, result[0].Title);
    }

    [Theory]
    [InlineData("just_a_filename.mp3", "Just A Filename")]
    [InlineData("kebab-case-name.mp3", "Kebab Case Name")]
    [InlineData(" - Leading Separator.mp3", "Leading Separator")]
    public void Scan_WithoutArtistSeparator_TitleCasesFilenameAndUsesUnknownArtist(string filename, string title)
    {
        var result = ScanFiles(filename);

        Assert.Equal("Unknown Artist", result[0].Artist);
        Assert.Equal(title, result[0].Title);
    }

    [Fact]
    public void Scan_OrdersFilesOrdinally()
    {
        // Ordinal comparison sorts by raw code point, so uppercase 'B'/'C'
        // (66/67) sort before lowercase 'a' (97) — a case-insensitive or
        // culture-aware sort would instead put "apple" first. Filenames must
        // differ by more than case: Windows/macOS default filesystems treat
        // two names that differ only in case as the same file.
        var result = ScanFiles("Cherry - Song.mp3", "apple - Song.mp3", "Banana - Song.mp3");

        Assert.Equal(
            new[] { "Banana - Song.mp3", "Cherry - Song.mp3", "apple - Song.mp3" },
            result.Select(t => $"{t.Artist} - {t.Title}.mp3"));
    }

    [Fact]
    public void Scan_SlugifiesId()
    {
        var result = ScanFiles("Digital Dust! - Track (Remix).mp3");

        Assert.Equal("digital-dust-track-remix", result[0].Id);
    }

    [Fact]
    public void Scan_EncodesSrcForUrlPathSegment()
    {
        var result = ScanFiles("Artist & Friends - Song #1.mp3");

        Assert.Equal($"/audio/{Uri.EscapeDataString("Artist & Friends - Song #1.mp3")}", result[0].Src);
        Assert.DoesNotContain(" ", result[0].Src);
    }

    [Fact]
    public void Scan_AssignsLabelColorFromPaletteConsistentlyForSameFilename()
    {
        var first = ScanFiles("Artist - Song.mp3");
        var second = ScanFiles("Artist - Song.mp3");

        Assert.Equal(first[0].LabelColor, second[0].LabelColor);
        Assert.StartsWith("#", first[0].LabelColor);
    }

    [Fact]
    public void Scan_DifferentFilenamesCanGetDifferentLabelColors()
    {
        var result = ScanFiles("Aardvark - One.mp3", "Zeppelin - Two.mp3", "Marmot - Three.mp3", "Osprey - Four.mp3");

        Assert.True(result.Select(t => t.LabelColor).Distinct().Count() > 1);
    }
}
