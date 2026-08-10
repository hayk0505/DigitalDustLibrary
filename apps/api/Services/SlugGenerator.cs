using System.Text.RegularExpressions;

namespace DigitalDustLibrary.Api.Services;

// Mirrors apps/admin/src/lib/formatting.ts's slugify() exactly (trim,
// lowercase, collapse non-alphanumeric runs to '-', trim leading/trailing
// '-') so both codebases produce identical slugs from identical input.
public static class SlugGenerator
{
    public static string Slugify(string input) =>
        Regex.Replace(input.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

    public static async Task<string> GenerateUniqueAsync(string baseInput, Func<string, Task<bool>> existsAsync)
    {
        var baseSlug = Slugify(baseInput);
        var candidate = baseSlug;
        var suffix = 2;
        while (await existsAsync(candidate))
        {
            candidate = $"{baseSlug}-{suffix}";
            suffix++;
        }
        return candidate;
    }
}
