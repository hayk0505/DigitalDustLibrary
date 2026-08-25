using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PostFormatPersistenceTests(ApiFactory factory)
{
    [Fact]
    public async Task CreatePost_WithMiniFormat_PersistsAndRetrievesCorrectly()
    {
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Format Author {suffix}", $"format-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(
            factory, author.Id, $"Format Test {suffix}", format: PostFormat.Mini);

        Assert.Equal(PostFormat.Mini, post.Format);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var retrieved = await db.Posts.SingleAsync(p => p.Id == post.Id);

        Assert.Equal(PostFormat.Mini, retrieved.Format);
    }

    [Fact]
    public async Task CreatePost_WithShelfFormat_PersistsAndRetrievesCorrectly()
    {
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Shelf Author {suffix}", $"shelf-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(
            factory, author.Id, $"Shelf Test {suffix}", format: PostFormat.Shelf);

        Assert.Equal(PostFormat.Shelf, post.Format);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var retrieved = await db.Posts.SingleAsync(p => p.Id == post.Id);

        Assert.Equal(PostFormat.Shelf, retrieved.Format);
    }

    [Fact]
    public async Task CreatePost_WithoutExplicitFormat_DefaultsToShelf()
    {
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Default Author {suffix}", $"default-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(
            factory, author.Id, $"Default Test {suffix}");

        Assert.Equal(PostFormat.Shelf, post.Format);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var retrieved = await db.Posts.SingleAsync(p => p.Id == post.Id);

        Assert.Equal(PostFormat.Shelf, retrieved.Format);
    }
}
