using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class PostDeleteTests(ApiFactory factory)
{
    [Fact]
    public async Task Delete_OwnPostAsAuthor_Returns204AndRemovesIt()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var createResponse = await author.PostAsJsonAsync("/api/posts",
            new CreatePostRequest($"Delete Me {suffix}", null, null, null, null, null, null, null));
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<PostDto>(AuthHelper.JsonOptions);

        var response = await author.DeleteAsync($"/api/posts/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.Posts.AnyAsync(p => p.Id == created.Id));
    }

    [Fact]
    public async Task Delete_AnotherAuthorsPostAsAuthor_Returns403AndDoesNotRemoveIt()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var otherAuthor = await UserTestHelpers.CreateUserAsync(
            factory, $"Other Author {suffix}", $"other-author-{suffix}@example.com", Role.Author);
        var othersPost = await PostTestHelpers.CreatePostAsync(factory, otherAuthor.Id, $"Not Yours {suffix}");

        var response = await author.DeleteAsync($"/api/posts/{othersPost.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await db.Posts.AnyAsync(p => p.Id == othersPost.Id));
    }

    [Fact]
    public async Task Delete_AnotherAuthorsPostAsEditor_Returns204()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Some Author {suffix}", $"some-author-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Editor Can Delete {suffix}");

        var response = await editor.DeleteAsync($"/api/posts/{post.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AnotherAuthorsPostAsOwner_Returns204()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = PostTestHelpers.UniqueSuffix();
        var author = await UserTestHelpers.CreateUserAsync(
            factory, $"Some Author {suffix}", $"some-author-owner-{suffix}@example.com", Role.Author);
        var post = await PostTestHelpers.CreatePostAsync(factory, author.Id, $"Owner Can Delete {suffix}");

        var response = await owner.DeleteAsync($"/api/posts/{post.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.DeleteAsync($"/api/posts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/posts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
