using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class CategoryUpdateTests(ApiFactory factory)
{
    [Fact]
    public async Task Patch_RenamesCategory()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var created = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Old Name {suffix}", $"old-slug-{suffix}");

        var response = await editor.PatchAsJsonAsync($"/api/categories/{created.Id}",
            new UpdateCategoryRequest($"New Name {suffix}", $"new-slug-{suffix}", null, null));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.Equal($"New Name {suffix}", updated!.Name);
        Assert.Equal($"new-slug-{suffix}", updated.Slug);
    }

    [Fact]
    public async Task Patch_SlugConflictingWithAnotherCategory_Returns409AndLeavesTargetUnchanged()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var takenSlug = $"taken-slug-{suffix}";
        await CategoryTestHelpers.CreateCategoryAsync(editor, $"Taken {suffix}", takenSlug);
        var target = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Target {suffix}", $"target-slug-{suffix}");

        var response = await editor.PatchAsJsonAsync($"/api/categories/{target.Id}",
            new UpdateCategoryRequest(null, takenSlug, null, null));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var stillThere = await CategoryTestHelpers.GetCategoryAsync(editor, target.Id);
        Assert.Equal($"target-slug-{suffix}", stillThere.Slug);
    }

    [Fact]
    public async Task Patch_TogglesVisibility()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var created = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Visibility Test {suffix}", $"visibility-{suffix}");

        var response = await editor.PatchAsJsonAsync($"/api/categories/{created.Id}",
            new UpdateCategoryRequest(null, null, false, null));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.False(updated!.IsVisible);
    }

    [Fact]
    public async Task Patch_SoftDeleteThenRestore_TogglesIsDeleted()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var created = await CategoryTestHelpers.CreateCategoryAsync(editor, $"SoftDelete Test {suffix}", $"softdelete-{suffix}");

        var deleteResponse = await editor.PatchAsJsonAsync($"/api/categories/{created.Id}",
            new UpdateCategoryRequest(null, null, null, true));
        deleteResponse.EnsureSuccessStatusCode();
        var deleted = await deleteResponse.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.True(deleted!.IsDeleted);

        var restoreResponse = await editor.PatchAsJsonAsync($"/api/categories/{created.Id}",
            new UpdateCategoryRequest(null, null, null, false));
        restoreResponse.EnsureSuccessStatusCode();
        var restored = await restoreResponse.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.False(restored!.IsDeleted);
    }

    [Fact]
    public async Task Patch_UnknownId_Returns404()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        var response = await editor.PatchAsJsonAsync($"/api/categories/{Guid.NewGuid()}",
            new UpdateCategoryRequest("Doesn't Matter", null, null, null));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Patch_WithoutAuthToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync($"/api/categories/{Guid.NewGuid()}",
            new UpdateCategoryRequest("No Auth", null, null, null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Patch_AsAuthor_Returns403()
    {
        var author = await AuthHelper.LoginAsAsync(factory, AuthHelper.AuthorEmail);

        var response = await author.PatchAsJsonAsync($"/api/categories/{Guid.NewGuid()}",
            new UpdateCategoryRequest("Author Attempt", null, null, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Patch_HidingCategory_LogsActivityEntry()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var created = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Hide Log {suffix}", $"hide-log-{suffix}");

        var response = await editor.PatchAsJsonAsync($"/api/categories/{created.Id}",
            new UpdateCategoryRequest(null, null, false, null));
        response.EnsureSuccessStatusCode();

        var activityResponse = await owner.GetAsync("/api/activity");
        var entries = await activityResponse.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.Contains(entries!, e => e.Action == $"hid \"Hide Log {suffix}\"");
    }

    [Fact]
    public async Task Patch_ArchivingCategory_LogsActivityEntry()
    {
        var editor = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);
        var suffix = CategoryTestHelpers.UniqueSuffix();
        var created = await CategoryTestHelpers.CreateCategoryAsync(editor, $"Archive Log {suffix}", $"archive-log-{suffix}");

        var response = await editor.PatchAsJsonAsync($"/api/categories/{created.Id}",
            new UpdateCategoryRequest(null, null, null, true));
        response.EnsureSuccessStatusCode();

        var activityResponse = await owner.GetAsync("/api/activity");
        var entries = await activityResponse.Content.ReadFromJsonAsync<List<ActivityEventDto>>(AuthHelper.JsonOptions);
        Assert.Contains(entries!, e => e.Action == $"archived \"Archive Log {suffix}\"");
    }
}
