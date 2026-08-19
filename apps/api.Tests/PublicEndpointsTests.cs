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

    // Regression test for a real prod incident: a Post row somehow ended up
    // Status=Published with PublishedAt=null (the only two write paths,
    // PATCH's self-publish and /approve, always set both together — how this
    // happened is still unconfirmed, but it happened), and
    // Mapping.ToPublicDto's `p.PublishedAt!.Value` force-unwrap threw
    // InvalidOperationException for every single request to GET
    // /api/public/posts, taking down the entire public blog homepage (no
    // global exception middleware exists yet, so this surfaced as a bare
    // 500 with no body). A single corrupt row must never be able to break
    // the public listing for every visitor.
    [Fact]
    public async Task GetPosts_PublishedPostWithNullPublishedAt_IsExcludedNotThrown()
    {
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Corrupt Author {suffix}", $"corrupt-author-{suffix}@example.com", Role.Author);
        await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Corrupt Post {suffix}", PostStatus.Published);

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/public/posts");

        response.EnsureSuccessStatusCode();
        var posts = await response.Content.ReadFromJsonAsync<List<PublicPostDto>>(AuthHelper.JsonOptions);
        Assert.DoesNotContain(posts!, p => p.Title == $"Corrupt Post {suffix}");
    }

    [Fact]
    public async Task GetPostBySlug_PublishedPostWithNullPublishedAt_Returns404NotThrown()
    {
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Corrupt Slug Author {suffix}", $"corrupt-slug-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Corrupt Slug Post {suffix}", PostStatus.Published);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var slug = (await db.Posts.SingleAsync(p => p.Id == post.Id)).Slug;

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/public/posts/{slug}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

    [Fact]
    public async Task GetPosts_WithCategoryFilter_ReturnsOnlyThatCategorysPosts()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Category Filter Author {suffix}", $"category-filter-author-{suffix}@example.com", Role.Author);
        var categoryA = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Filter A {suffix}", $"filter-a-{suffix}");
        var categoryB = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Filter B {suffix}", $"filter-b-{suffix}");
        var postInA = await PostTestHelpers.CreatePostAsync(
            factory, author.Id, $"In A {suffix}", PostStatus.PendingReview, categoryId: categoryA.Id);
        await editor.PostAsync($"/api/posts/{postInA.Id}/approve", null);
        var postInB = await PostTestHelpers.CreatePostAsync(
            factory, author.Id, $"In B {suffix}", PostStatus.PendingReview, categoryId: categoryB.Id);
        await editor.PostAsync($"/api/posts/{postInB.Id}/approve", null);

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/public/posts?category={categoryA.Slug}");

        response.EnsureSuccessStatusCode();
        var posts = await response.Content.ReadFromJsonAsync<List<PublicPostDto>>(AuthHelper.JsonOptions);
        Assert.Contains(posts!, p => p.Title == $"In A {suffix}");
        Assert.DoesNotContain(posts!, p => p.Title == $"In B {suffix}");
    }

    [Fact]
    public async Task GetCategories_ReturnsDescriptionColorPositionAndPublishedPostCount()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Category Shape Author {suffix}", $"category-shape-author-{suffix}@example.com", Role.Author);
        var category = await CategoryTestHelpers.CreateCategoryAsync(
            editor, $"Shape Test {suffix}", $"shape-test-{suffix}", "A real description.", "#654321", position: 777);
        var post = await PostTestHelpers.CreatePostAsync(
            factory, author.Id, $"Shape Post {suffix}", PostStatus.PendingReview, categoryId: category.Id);
        await editor.PostAsync($"/api/posts/{post.Id}/approve", null);

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/public/categories");

        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<List<PublicCategoryDto>>(AuthHelper.JsonOptions);
        var found = categories!.Single(c => c.Slug == $"shape-test-{suffix}");
        Assert.Equal("A real description.", found.Description);
        Assert.Equal("#654321", found.Color);
        Assert.Equal(777, found.Position);
        Assert.Equal(1, found.PostCount);
    }

    [Fact]
    public async Task GetTags_ReturnsPublishedPostCountOnly()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Public Tag Author {suffix}", $"public-tag-author-{suffix}@example.com", Role.Author);
        var tag = await TagTestHelpers.CreateTagAsync(editor, $"Public Tag {suffix}");
        var publishedPost = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Public Tagged {suffix}", PostStatus.PendingReview);
        await DbTestHelpers.AddPostTagAsync(factory, publishedPost.Id, tag.Id);
        await editor.PostAsync($"/api/posts/{publishedPost.Id}/approve", null);
        var draftPost = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Draft Tagged {suffix}", PostStatus.Draft);
        await DbTestHelpers.AddPostTagAsync(factory, draftPost.Id, tag.Id);

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/public/tags");

        response.EnsureSuccessStatusCode();
        var tags = await response.Content.ReadFromJsonAsync<List<PublicTagDto>>(AuthHelper.JsonOptions);
        var found = tags!.Single(t => t.Slug == tag.Slug);
        Assert.Equal(1, found.PostCount);
    }

    [Fact]
    public async Task GetPosts_WithTagFilter_ReturnsOnlyThatTagsPosts()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Tag Filter Author {suffix}", $"tag-filter-author-{suffix}@example.com", Role.Author);
        var tagA = await TagTestHelpers.CreateTagAsync(editor, $"Filter Tag A {suffix}");
        var tagB = await TagTestHelpers.CreateTagAsync(editor, $"Filter Tag B {suffix}");
        var postInA = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Tagged A {suffix}", PostStatus.PendingReview);
        await DbTestHelpers.AddPostTagAsync(factory, postInA.Id, tagA.Id);
        await editor.PostAsync($"/api/posts/{postInA.Id}/approve", null);
        var postInB = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Tagged B {suffix}", PostStatus.PendingReview);
        await DbTestHelpers.AddPostTagAsync(factory, postInB.Id, tagB.Id);
        await editor.PostAsync($"/api/posts/{postInB.Id}/approve", null);

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/public/posts?tag={tagA.Slug}");

        response.EnsureSuccessStatusCode();
        var posts = await response.Content.ReadFromJsonAsync<List<PublicPostDto>>(AuthHelper.JsonOptions);
        Assert.Contains(posts!, p => p.Title == $"Tagged A {suffix}");
        Assert.DoesNotContain(posts!, p => p.Title == $"Tagged B {suffix}");
    }

    [Fact]
    public async Task GetPostBySlug_IncludesTags()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Slug Tags Author {suffix}", $"slug-tags-author-{suffix}@example.com", Role.Author);
        var tag = await TagTestHelpers.CreateTagAsync(editor, $"Slug Tag {suffix}");
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Slug Tags Post {suffix}", PostStatus.PendingReview);
        await DbTestHelpers.AddPostTagAsync(factory, post.Id, tag.Id);
        await editor.PostAsync($"/api/posts/{post.Id}/approve", null);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var slug = (await db.Posts.SingleAsync(p => p.Id == post.Id)).Slug;

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/public/posts/{slug}");

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<PublicPostDto>(AuthHelper.JsonOptions);
        Assert.Single(dto!.Tags);
        Assert.Equal(tag.Slug, dto.Tags[0].Slug);
    }
}
