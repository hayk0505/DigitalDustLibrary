using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class UserDeleteTests(ApiFactory factory)
{
    private async Task<ApplicationUser> CreateAuthorAsync(string suffix)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new ApplicationUser
        {
            Name = $"Delete Target {suffix}",
            Handle = $"delete-target-{suffix}",
            Email = $"delete-target-{suffix}@example.com",
            Role = Role.Author,
            IsActive = true,
            PasswordHash = "unused-in-these-tests",
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task Delete_UserWithPostsMediaAndReviewNotes_CascadesEverythingCorrectly()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var target = await CreateAuthorAsync(suffix);

        string mediaFilePath;
        Guid ownPostId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            // Post.CategoryId is non-nullable now — a throwaway category just
            // to satisfy the FK, this test doesn't care which one.
            var category = new Category
            {
                Name = $"UserDelete Test Category {suffix}", Slug = $"user-delete-test-category-{suffix}",
                Description = "Throwaway category for UserDeleteTests.", Color = "#A27B5B",
            };
            db.Categories.Add(category);

            var ownPost = new Post { Title = $"Own Post {suffix}", Slug = $"own-post-{suffix}", AuthorId = target.Id, CategoryId = category.Id };
            db.Posts.Add(ownPost);
            ownPostId = ownPost.Id;

            var uploadsDir = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsDir);
            var storedFilename = $"{suffix}.png";
            mediaFilePath = Path.Combine(uploadsDir, storedFilename);
            await File.WriteAllBytesAsync(mediaFilePath, [1, 2, 3, 4]);

            var media = new MediaAsset
            {
                Filename = "test.png", Tag = MediaTag.Featured, Width = 10, Height = 10,
                Url = $"http://localhost/uploads/{storedFilename}", UploadedById = target.Id,
            };
            db.MediaAssets.Add(media);

            // A post belonging to someone ELSE, which the target reviewed, and which
            // also uses the target's media asset as its featured image — must survive
            // the target's deletion (the review note goes; the featured image
            // reference gets nulled out via the FK's SetNull behavior, since the
            // MediaAsset itself is hard-deleted along with the target).
            var otherAuthor = await CreateAuthorAsync($"other-{suffix}");
            var otherPost = new Post
            {
                Title = $"Other Post {suffix}", Slug = $"other-post-{suffix}", AuthorId = otherAuthor.Id,
                FeaturedImageId = media.Id, CategoryId = category.Id,
            };
            db.Posts.Add(otherPost);
            db.ReviewNotes.Add(new ReviewNote { PostId = otherPost.Id, ReviewerId = target.Id, Comment = "Reviewed by target" });

            // A review note written by a THIRD user on the target's own post — must be
            // cascade-removed too (via Posts -> ReviewNotes ON DELETE CASCADE) once the
            // target's own post is deleted, even though the reviewer isn't being deleted.
            var thirdReviewer = await CreateAuthorAsync($"third-{suffix}");
            db.ReviewNotes.Add(new ReviewNote { PostId = ownPost.Id, ReviewerId = thirdReviewer.Id, Comment = "Reviewed by a third party" });

            await db.SaveChangesAsync();
        }

        // Called BEFORE deletion — proves the impact endpoint warns about the
        // cross-author featured-image collateral damage ahead of time.
        var impactResponse = await owner.GetAsync($"/api/users/{target.Id}/deletion-impact");
        impactResponse.EnsureSuccessStatusCode();
        var impact = await impactResponse.Content.ReadFromJsonAsync<UserDeletionImpactDto>(AuthHelper.JsonOptions);
        Assert.Equal(1, impact!.AffectedOtherPostCount);

        var response = await owner.DeleteAsync($"/api/users/{target.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await verifyDb.Users.AnyAsync(u => u.Id == target.Id));
        Assert.False(await verifyDb.Posts.AnyAsync(p => p.AuthorId == target.Id));
        Assert.False(await verifyDb.MediaAssets.AnyAsync(m => m.UploadedById == target.Id));
        Assert.False(await verifyDb.ReviewNotes.AnyAsync(r => r.ReviewerId == target.Id));
        Assert.False(File.Exists(mediaFilePath)); // the physical file must actually be gone, not just the DB row
        // the other author's post itself must be untouched, but loses its featured image
        var survivingOtherPost = await verifyDb.Posts.SingleAsync(p => p.Slug == $"other-post-{suffix}");
        Assert.Null(survivingOtherPost.FeaturedImageId);
        // the third reviewer's note on the target's own (now-deleted) post must be gone too
        Assert.False(await verifyDb.ReviewNotes.AnyAsync(r => r.PostId == ownPostId));
        // the deletion itself must be recorded in the activity log
        Assert.True(await verifyDb.ActivityLog.AnyAsync(e => e.Action == $"deleted {target.Name}'s account"));
    }

    [Fact]
    public async Task Delete_UserWhoActedInActivityLog_PreservesEntryWithNullActor()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var target = await CreateAuthorAsync(suffix);

        // Seed the activity log entry directly via the DB (as if `target` had
        // taken some logged action in the past) — no need to actually log in
        // as them or call a real endpoint; this test only cares about what
        // happens to the entry when `target` is later deleted.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ActivityLog.Add(new ActivityLogEntry { ActorId = target.Id, Action = $"did something {suffix}" });
            await db.SaveChangesAsync();
        }

        var response = await owner.DeleteAsync($"/api/users/{target.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var activityResponse = await owner.GetAsync("/api/activity");
        var entries = await activityResponse.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        var entry = entries!.Single(e => e.Action == $"did something {suffix}");
        Assert.Equal("Deleted user", entry.ActorName);
    }

    [Fact]
    public async Task Delete_Self_Returns409()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ownerUser = await db.Users.SingleAsync(u => u.Email == AuthHelper.OwnerEmail);

        var response = await owner.DeleteAsync($"/api/users/{ownerUser.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);

        var response = await owner.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsEditor_Returns403()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var target = await CreateAuthorAsync(suffix);

        var response = await editor.DeleteAsync($"/api/users/{target.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeletionImpact_ReturnsCorrectCounts()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = ApplicationTestHelpers.UniqueSuffix();
        var target = await CreateAuthorAsync(suffix);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Post.CategoryId is non-nullable now — a throwaway category just
            // to satisfy the FK, this test doesn't care which one.
            var category = new Category
            {
                Name = $"Impact Test Category {suffix}", Slug = $"impact-test-category-{suffix}",
                Description = "Throwaway category for UserDeleteTests.", Color = "#A27B5B",
            };
            db.Categories.Add(category);
            db.Posts.Add(new Post { Title = $"Impact Post {suffix}", Slug = $"impact-post-{suffix}", AuthorId = target.Id, CategoryId = category.Id });
            db.MediaAssets.Add(new MediaAsset
            {
                Filename = "impact.png", Tag = MediaTag.Featured, Width = 10, Height = 10,
                Url = $"http://localhost/uploads/impact-{suffix}.png", UploadedById = target.Id,
            });
            await db.SaveChangesAsync();
        }

        var response = await owner.GetAsync($"/api/users/{target.Id}/deletion-impact");

        response.EnsureSuccessStatusCode();
        var impact = await response.Content.ReadFromJsonAsync<UserDeletionImpactDto>(AuthHelper.JsonOptions);
        Assert.Equal(1, impact!.PostCount);
        Assert.Equal(1, impact.MediaCount);
        Assert.Equal(0, impact.ReviewNoteCount);
        // no other author's post uses this user's media as its featured image here
        Assert.Equal(0, impact.AffectedOtherPostCount);
    }
}
