using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

public static class PostTestHelpers
{
    // Direct DB insert so tests can create a post owned by an arbitrary
    // author — the only way to create a post through the API creates it
    // owned by the caller, which isn't enough for cross-author 403 tests.
    public static async Task<Post> CreatePostAsync(
        ApiFactory factory, Guid authorId, string title, PostStatus status = PostStatus.Draft)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var slug = await SlugGenerator.GenerateUniqueAsync(title, s => db.Posts.AnyAsync(p => p.Slug == s));
        var post = new Post { Title = title, Slug = slug, AuthorId = authorId, Status = status };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return post;
    }

    public static string UniqueSuffix() => Guid.NewGuid().ToString("N")[..12];
}
