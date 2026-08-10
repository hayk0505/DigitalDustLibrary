namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class AuthHelperTests(ApiFactory factory)
{
    [Fact]
    public async Task LoginAsAsync_EditorAccount_ReturnsClientWithBearerToken()
    {
        var client = await AuthHelper.LoginAsAsync(factory, AuthHelper.EditorEmail);

        Assert.NotNull(client.DefaultRequestHeaders.Authorization);
        Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization!.Scheme);
        Assert.False(string.IsNullOrEmpty(client.DefaultRequestHeaders.Authorization.Parameter));
    }
}
