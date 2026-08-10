using System.Net;
using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

[Collection(ApiCollection.Name)]
public class AuthRefreshDeactivationTests(ApiFactory factory)
{
    [Fact]
    public async Task Refresh_UserDeactivatedButTokenNotRevoked_Returns401()
    {
        var suffix = UserTestHelpers.UniqueSuffix();
        var target = await UserTestHelpers.CreateUserAsync(
            factory, $"Defense Test {suffix}", $"defense-{suffix}@example.com", Role.Author);

        string rawToken;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            rawToken = TokenService.GenerateRefreshTokenValue();
            db.RefreshTokens.Add(new RefreshToken
            {
                UserId = target.Id,
                TokenHash = TokenService.HashToken(rawToken),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            });
            var user = await db.Users.SingleAsync(u => u.Id == target.Id);
            user.IsActive = false; // deactivated directly, bypassing PATCH's own revocation
            await db.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refreshRequest.Headers.Add("Cookie", $"refreshToken={Uri.EscapeDataString(rawToken)}");
        var response = await client.SendAsync(refreshRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
