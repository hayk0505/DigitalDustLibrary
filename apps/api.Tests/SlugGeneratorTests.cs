using DigitalDustLibrary.Api.Services;

namespace DigitalDustLibrary.Api.Tests;

public class SlugGeneratorTests
{
    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("  Trim Me  ", "trim-me")]
    [InlineData("Tech & Craft!!", "tech-craft")]
    [InlineData("Already-Hyphenated", "already-hyphenated")]
    public void Slugify_ProducesExpectedOutput(string input, string expected)
    {
        Assert.Equal(expected, SlugGenerator.Slugify(input));
    }

    [Fact]
    public async Task GenerateUniqueAsync_ReturnsBaseSlugWhenAvailable()
    {
        var result = await SlugGenerator.GenerateUniqueAsync("New Post", _ => Task.FromResult(false));

        Assert.Equal("new-post", result);
    }

    [Fact]
    public async Task GenerateUniqueAsync_AppendsSuffixWhenTaken()
    {
        var existing = new HashSet<string> { "new-post" };

        var result = await SlugGenerator.GenerateUniqueAsync("New Post", slug => Task.FromResult(existing.Contains(slug)));

        Assert.Equal("new-post-2", result);
    }

    [Fact]
    public async Task GenerateUniqueAsync_IncrementsPastMultipleCollisions()
    {
        var existing = new HashSet<string> { "new-post", "new-post-2", "new-post-3" };

        var result = await SlugGenerator.GenerateUniqueAsync("New Post", slug => Task.FromResult(existing.Contains(slug)));

        Assert.Equal("new-post-4", result);
    }
}
