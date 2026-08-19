namespace DigitalDustLibrary.Api.Services;

public record AudioTrack(string Id, string Title, string Artist, string Src, string LabelColor);

/// <summary>
/// Scans wwwroot/audio (see Program.cs's static file mapping for /audio, and
/// docker-compose.prod.yml's audio-files bind mount) and builds the sidebar
/// turntable's playlist. This is a straight C# port of the logic that used
/// to live in apps/blog/vite-plugins/audio-playlist.ts, moved server-side
/// because that Vite plugin scanned a local static/ folder at build time —
/// which required the audio files to be committed to git so CI's checkout
/// had them. Now the files live only on the droplet (scp'd directly into
/// audio-files/, never committed) and the blog fetches this endpoint at
/// request/build time instead of reading a local folder that no longer
/// exists in its own repo. Keep this in sync with that file's naming rules
/// if either ever changes — same "Artist - Title.ext" convention, same
/// extension whitelist, same label-color hash, so a track looks identical
/// however it happens to be discovered.
/// </summary>
public static class AudioTrackScanner
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".m4a", ".aac", ".ogg", ".oga", ".opus", ".wav", ".flac", ".webm",
    };

    // Same muted record-label palette as the Vite plugin, so a given filename
    // always lands on the same colour no matter which side generates it.
    private static readonly string[] LabelColors =
    [
        "#C9553D", "#5B6B4E", "#7C8B96", "#8B5E3C", "#9C8465", "#6B7A8F",
    ];

    public static List<AudioTrack> Scan(string audioDir)
    {
        if (!Directory.Exists(audioDir)) return [];

        return Directory.EnumerateFiles(audioDir)
            .Select(Path.GetFileName)
            .Where(name => name is not null && AllowedExtensions.Contains(Path.GetExtension(name)))
            .Select(name => name!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .Select(ToTrack)
            .ToList();
    }

    private static AudioTrack ToTrack(string filename)
    {
        var ext = Path.GetExtension(filename);
        var baseName = filename[..^ext.Length];

        var separatorIndex = baseName.IndexOf(" - ", StringComparison.Ordinal);
        var hasArtist = separatorIndex > 0;

        var title = hasArtist ? baseName[(separatorIndex + 3)..].Trim() : TitleCase(baseName);
        var artist = hasArtist ? baseName[..separatorIndex].Trim() : "Unknown Artist";

        return new AudioTrack(
            Id: Slugify(baseName),
            Title: title,
            Artist: artist,
            // Encoded for the URL path segment — filenames are user-supplied
            // (whatever the track was named on disk), not pre-sanitized slugs.
            Src: $"/audio/{Uri.EscapeDataString(filename)}",
            LabelColor: LabelColors[Math.Abs(HashString(baseName)) % LabelColors.Length]);
    }

    private static int HashString(string seed)
    {
        var hash = 0;
        foreach (var c in seed) hash = unchecked(hash * 31 + c);
        return hash;
    }

    private static string Slugify(string value)
    {
        var lowered = value.ToLowerInvariant();
        var slug = System.Text.RegularExpressions.Regex.Replace(lowered, "[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "track" : slug;
    }

    private static string TitleCase(string value)
    {
        var spaced = System.Text.RegularExpressions.Regex.Replace(value, "[_-]+", " ");
        spaced = System.Text.RegularExpressions.Regex.Replace(spaced, @"\s+", " ").Trim();
        return System.Text.RegularExpressions.Regex.Replace(spaced, @"\b\w", m => m.Value.ToUpperInvariant());
    }
}
