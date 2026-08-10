using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PublicEndpointsTests(ApiFactory factory)
{
    [Fact]
    public async Task GetPosts_NoAuthToken_ReturnsPublishedPostsOnly()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Public Author {suffix}", $"public-author-{suffix}@example.com", Role.Author);
        var publishedPost = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Public Post {suffix}", PostStatus.PendingReview);
        await editor.PostAsync($"/api/posts/{publishedPost.Id}/approve", null);
        await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Secret Draft {suffix}", PostStatus.Draft);

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/public/posts");

        response.EnsureSuccessStatusCode();
        var posts = await response.Content.ReadFromJsonAsync<List<PublicPostDto>>(AuthHelper.JsonOptions);
        Assert.Contains(posts!, p => p.Title == $"Public Post {suffix}");
        Assert.DoesNotContain(posts!, p => p.Title == $"Secret Draft {suffix}");
    }

    [Fact]
    public async Task GetPostBySlug_UnknownSlug_Returns404()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/public/posts/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostBySlug_UnpublishedPost_Returns404()
    {
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Unpub Author {suffix}", $"unpub-author-{suffix}@example.com", Role.Author);
        var draftPost = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Unpublished {suffix}", PostStatus.Draft);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var slug = (await db.Posts.SingleAsync(p => p.Id == draftPost.Id)).Slug;

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/public/posts/{slug}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPostBySlug_PublishedPost_ReturnsExpectedShape()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Shape Author {suffix}", $"shape-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Shape Test {suffix}", PostStatus.PendingReview);
        await editor.PostAsync($"/api/posts/{post.Id}/approve", null);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var slug = (await db.Posts.SingleAsync(p => p.Id == post.Id)).Slug;

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/public/posts/{slug}");

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PublicPostDto>(AuthHelper.JsonOptions);
        Assert.Equal($"Shape Test {suffix}", dto!.Title);
        Assert.True(dto.ReadingMinutes >= 1);
        Assert.True(dto.DispatchNumber >= 1);
    }

    [Fact]
    public async Task GetCategories_ExcludesHiddenAndDeleted()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        await CategoryTestHelpers.CreateCategoryAsync(editor, $"Public Visible {suffix}", $"public-visible-{suffix}");
        var hidden = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Public Hidden {suffix}", $"public-hidden-{suffix}");
        await editor.PatchAsJsonAsync($"/api/categories/{hidden.Id}", new UpdateCategoryRequest(null, null, false, null));

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/public/categories");

        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<List<PublicCategoryDto>>(AuthHelper.JsonOptions);
        Assert.Contains(categories!, c => c.Slug == $"public-visible-{suffix}");
        Assert.DoesNotContain(categories!, c => c.Slug == $"public-hidden-{suffix}");
    }

    [Fact]
    public async Task GetAuthorByHandle_UnknownHandle_Returns404()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/public/authors/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuthorByHandle_KnownHandle_ReturnsNameAndHandle()
    {
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Handle Author {suffix}", $"handle-author-{suffix}@example.com", Role.Author);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var handle = (await db.Users.SingleAsync(u => u.Id == author.Id)).Handle;

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/public/authors/{handle}");

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PublicAuthorDto>(AuthHelper.JsonOptions);
        Assert.Equal($"Handle Author {suffix}", dto!.Name);
    }
}
