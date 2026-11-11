using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Models;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class MediaDeleteTests(ApiFactory factory)
{
    private const string OnePixelPngDataUrl =
        "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";

    private static async Task<MediaAssetDto> UploadAsync(HttpClient client, string filename)
    {
        var response = await client.PostAsJsonAsync("/api/media",
            new CreateMediaRequest(filename, OnePixelPngDataUrl, MediaTag.Inline, 1, 1), AuthHelper.JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MediaAssetDto>(AuthHelper.JsonOptions))!;
    }

    [Fact]
    public async Task Delete_ExistingAsset_Returns204AndRemovesFileFromDisk()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var asset = await UploadAsync(editor, "delete-me.png");

        var response = await editor.DeleteAsync($"/api/media/{asset.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var listResponse = await editor.GetAsync("/api/media");
        var remaining = await listResponse.Content.ReadFromJsonAsync<List<MediaAssetDto>>(AuthHelper.JsonOptions);
        Assert.DoesNotContain(remaining!, m => m.Id == asset.Id);

        using var anonymousClient = factory.CreateClient();
        var fileResponse = await anonymousClient.GetAsync(asset.Url);
        Assert.Equal(HttpStatusCode.NotFound, fileResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_AssetReferencedByPost_Returns204AndClearsFeaturedImageId()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var asset = await UploadAsync(owner, "featured.png");
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var category = await CategoryTestHelpers.CreateCategoryAsync(owner, $"Media Delete {suffix}", $"media-delete-{suffix}");
        var postId = await DbTestHelpers.AddPostReferencingMediaAsync(factory, category.Id, asset.Id);

        var response = await owner.DeleteAsync($"/api/media/{asset.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var listResponse = await owner.GetAsync("/api/posts");
        var posts = await listResponse.Content.ReadFromJsonAsync<List<PostDto>>(AuthHelper.JsonOptions);
        var post = posts!.Single(p => p.Id == postId);
        Assert.Null(post.FeaturedImageId);
    }

    [Fact]
    public async Task Delete_AssetFeaturedOnPublishedPost_Returns409AndKeepsAsset()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var asset = await UploadAsync(owner, "live-featured.png");
        var category = await CreateThrowawayCategoryAsync(owner);
        var title = $"Live featured {CategoryTestHelpers.UniqueSuffix()}";
        await DbTestHelpers.AddPostUsingMediaAsync(
            factory, category.Id, PostStatus.Published, title, featuredImageId: asset.Id);

        var response = await owner.DeleteAsync($"/api/media/{asset.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(title, body.GetProperty("message").GetString());
        await AssertAssetStillPresentAsync(owner, asset);
    }

    [Fact]
    public async Task Delete_AssetInlineInPublishedPostBody_Returns409AndKeepsAsset()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var asset = await UploadAsync(owner, "live-inline.png");
        var category = await CreateThrowawayCategoryAsync(owner);
        var title = $"Live inline {CategoryTestHelpers.UniqueSuffix()}";
        await DbTestHelpers.AddPostUsingMediaAsync(
            factory, category.Id, PostStatus.Published, title,
            bodyHtml: $"<p>Intro</p><img src=\"{asset.Url}\" alt=\"inline\">");

        var response = await owner.DeleteAsync($"/api/media/{asset.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(title, body.GetProperty("message").GetString());
        await AssertAssetStillPresentAsync(owner, asset);
    }

    [Fact]
    public async Task Delete_AssetInlineInDraftBodyOnly_Returns204()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var asset = await UploadAsync(owner, "draft-inline.png");
        var category = await CreateThrowawayCategoryAsync(owner);
        await DbTestHelpers.AddPostUsingMediaAsync(
            factory, category.Id, PostStatus.Draft, $"Draft inline {CategoryTestHelpers.UniqueSuffix()}",
            bodyHtml: $"<img src=\"{asset.Url}\" alt=\"inline\">");

        var response = await owner.DeleteAsync($"/api/media/{asset.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static Task<CategoryDto> CreateThrowawayCategoryAsync(HttpClient client)
    {
        var suffix = CategoryTestHelpers.UniqueSuffix();
        return CategoryTestHelpers.CreateCategoryAsync(client, $"Media Delete {suffix}", $"media-delete-{suffix}");
    }

    private async Task AssertAssetStillPresentAsync(HttpClient client, MediaAssetDto asset)
    {
        var listResponse = await client.GetAsync("/api/media");
        var assets = await listResponse.Content.ReadFromJsonAsync<List<MediaAssetDto>>(AuthHelper.JsonOptions);
        Assert.Contains(assets!, m => m.Id == asset.Id);

        using var anonymousClient = factory.CreateClient();
        var fileResponse = await anonymousClient.GetAsync(asset.Url);
        Assert.Equal(HttpStatusCode.OK, fileResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.DeleteAsync($"/api/media/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/media/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.DeleteAsync($"/api/media/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
