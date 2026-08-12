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
    // categoryId is optional: when omitted, a fresh throwaway category is
    // created for this one post (matching the fresh-slug-per-call pattern
    // already used below) — CategoryId is required on Post now, and no
    // caller of this helper actually cares which category, only that one
    // exists.
    public static async Task<Post> CreatePostAsync(
        ApiFactory factory, Guid authorId, string title, PostStatus status = PostStatus.Draft, Guid? categoryId = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var slug = await SlugGenerator.GenerateUniqueAsync(title, s => db.Posts.AnyAsync(p => p.Slug == s));
        var resolvedCategoryId = categoryId ?? (await CreateThrowawayCategoryAsync(db)).Id;
        var post = new Post { Title = title, Slug = slug, AuthorId = authorId, Status = status, CategoryId = resolvedCategoryId };
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
