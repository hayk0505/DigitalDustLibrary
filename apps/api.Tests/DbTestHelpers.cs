using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

public static class DbTestHelpers
{
    // The Post-creation API doesn't expose CategoryId in its Phase 1 contract
    // (see Models/Post.cs), so this inserts directly via the app's own
    // AppDbContext to get a Post referencing a Category for delete-blocked tests.
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

    public static async Task AddPostTagAsync(ApiFactory factory, Guid postId, Guid tagId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.PostTags.Add(new PostTag { PostId = postId, TagId = tagId });
        await db.SaveChangesAsync();
    }
}
