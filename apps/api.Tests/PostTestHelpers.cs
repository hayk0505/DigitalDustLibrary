using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

public static class PostTestHelpers
{
    public static async Task<Post> CreatePostAsync(
        ApiFactory factory, Guid authorId, string title, PostStatus status = PostStatus.Draft,
        Guid? categoryId = null, string? excerpt = null, string? bodyHtml = null, PostFormat? format = null,
        string? source = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var slug = await SlugGenerator.GenerateUniqueAsync(title, s => db.Posts.AnyAsync(p => p.Slug == s));
        var resolvedCategoryId = categoryId ?? (await CreateThrowawayCategoryAsync(db)).Id;
        var post = new Post
        {
            Title = title, Slug = slug, AuthorId = authorId, Status = status, CategoryId = resolvedCategoryId,
            Excerpt = excerpt ?? "", BodyHtml = bodyHtml ?? "", Format = format ?? PostFormat.Shelf,
            Source = source,
        };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return post;
    }

    private static async Task<Category> CreateThrowawayCategoryAsync(AppDbContext db)
    {
        var suffix = UniqueSuffix();
        var category = new Category
        {
            Name = $"Test Category {suffix}",
            Slug = $"test-category-{suffix}",
            Description = "Throwaway category for a test post.",
            Color = "#A27B5B",
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public static string UniqueSuffix() => Guid.NewGuid().ToString("N")[..12];
}
