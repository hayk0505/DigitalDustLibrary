using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

public static class DbTestHelpers
{
    public static async Task AddPostReferencingCategoryAsync(ApiFactory factory, Guid categoryId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var author = await db.Users.SingleAsync(u => u.Email == AuthHelper.AuthorEmail);
        var title = $"Post referencing category {categoryId}";
        var slug = await SlugGenerator.GenerateUniqueAsync(title, s => db.Posts.AnyAsync(p => p.Slug == s));

        db.Posts.Add(new Post
        {
            Title = title,
            Slug = slug,
            AuthorId = author.Id,
            CategoryId = categoryId,
        });
        await db.SaveChangesAsync();
    }

    public static async Task<Guid> AddPostReferencingMediaAsync(ApiFactory factory, Guid categoryId, Guid mediaAssetId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var author = await db.Users.SingleAsync(u => u.Email == AuthHelper.AuthorEmail);
        var title = $"Post referencing media {mediaAssetId}";
        var slug = await SlugGenerator.GenerateUniqueAsync(title, s => db.Posts.AnyAsync(p => p.Slug == s));

        var post = new Post
        {
            Title = title,
            Slug = slug,
            AuthorId = author.Id,
            CategoryId = categoryId,
            FeaturedImageId = mediaAssetId,
        };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return post.Id;
    }

    // Post that uses a media asset the way a real one would: as its featured
    // image, inline in the body, or both, in whatever status the test needs.
    public static async Task<Guid> AddPostUsingMediaAsync(
        ApiFactory factory, Guid categoryId, PostStatus status, string title,
        Guid? featuredImageId = null, string bodyHtml = "")
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var author = await db.Users.SingleAsync(u => u.Email == AuthHelper.AuthorEmail);
        var slug = await SlugGenerator.GenerateUniqueAsync(title, s => db.Posts.AnyAsync(p => p.Slug == s));

        var post = new Post
        {
            Title = title,
            Slug = slug,
            AuthorId = author.Id,
            CategoryId = categoryId,
            FeaturedImageId = featuredImageId,
            BodyHtml = bodyHtml,
            Status = status,
            PublishedAt = status == PostStatus.Published ? DateTimeOffset.UtcNow : null,
        };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return post.Id;
    }

    public static async Task AddPostTagAsync(ApiFactory factory, Guid postId, Guid tagId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PostTags.Add(new PostTag { PostId = postId, TagId = tagId });
        await db.SaveChangesAsync();
    }
}
