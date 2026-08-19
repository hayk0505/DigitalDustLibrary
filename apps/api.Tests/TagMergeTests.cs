using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class TagMergeTests(ApiFactory factory)
{
    [Fact]
    public async Task Merge_ReassignsPostsAndDeletesSource()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Merge Author {suffix}", $"merge-author-{suffix}@example.com", Role.Author);
        var source = await TagTestHelpers.CreateTagAsync(editor, $"Source {suffix}");
        var target = await TagTestHelpers.CreateTagAsync(editor, $"Target {suffix}");
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Merge Post {suffix}");
        await DbTestHelpers.AddPostTagAsync(factory, post.Id, source.Id);

        var response = await editor.PostAsJsonAsync($"/api/tags/{source.Id}/merge", new MergeTagRequest(target.Id));

        response.EnsureSuccessStatusCode();
        var mergedTarget = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.Equal(1, mergedTarget!.PostCount);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.Tags.AnyAsync(t => t.Id == source.Id), "expected the source tag to be deleted");
        Assert.True(await db.PostTags.AnyAsync(pt => pt.PostId == post.Id && pt.TagId == target.Id));
    }

    [Fact]
    public async Task Merge_PostAlreadyHasBothTags_DedupesInsteadOfViolatingUniqueConstraint()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Dedup Author {suffix}", $"dedup-author-{suffix}@example.com", Role.Author);
        var source = await TagTestHelpers.CreateTagAsync(editor, $"Dedup Source {suffix}");
        var target = await TagTestHelpers.CreateTagAsync(editor, $"Dedup Target {suffix}");
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Dedup Post {suffix}");
        await DbTestHelpers.AddPostTagAsync(factory, post.Id, source.Id);
        await DbTestHelpers.AddPostTagAsync(factory, post.Id, target.Id);

        var response = await editor.PostAsJsonAsync($"/api/tags/{source.Id}/merge", new MergeTagRequest(target.Id));

        response.EnsureSuccessStatusCode();
        var mergedTarget = await response.Content.ReadFromJsonAsync<TagDto>();
        Assert.Equal(1, mergedTarget!.PostCount);
    }

    [Fact]
    public async Task Merge_IntoSelf_Returns400()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var tag = await TagTestHelpers.CreateTagAsync(editor, $"Self Merge {TagTestHelpers.UniqueSuffix()}");

        var response = await editor.PostAsJsonAsync($"/api/tags/{tag.Id}/merge", new MergeTagRequest(tag.Id));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Merge_UnknownTargetId_Returns404()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var source = await TagTestHelpers.CreateTagAsync(editor, $"Unknown Target Source {TagTestHelpers.UniqueSuffix()}");

        var response = await editor.PostAsJsonAsync($"/api/tags/{source.Id}/merge", new MergeTagRequest(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Merge_AsAuthor_Returns403()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = TagTestHelpers.UniqueSuffix();
        var source = await TagTestHelpers.CreateTagAsync(editor, $"Auth Source {suffix}");
        var target = await TagTestHelpers.CreateTagAsync(editor, $"Auth Target {suffix}");

        var response = await author.PostAsJsonAsync($"/api/tags/{source.Id}/merge", new MergeTagRequest(target.Id));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
