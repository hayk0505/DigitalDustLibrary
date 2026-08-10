using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

public static class ApplicationTestHelpers
{
    // Bypasses the rate-limited public POST /api/applications endpoint for
    // test setup that isn't itself testing submission/rate-limiting.
    public static async Task<AuthorApplication> CreatePendingApplicationAsync(
        ApiFactory factory, string name, string email, string pitch)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var application = new AuthorApplication { Name = name, Email = email, Pitch = pitch };
        db.AuthorApplications.Add(application);
        await db.SaveChangesAsync();
        return application;
    }

    public static string UniqueSuffix() => Guid.NewGuid().ToString("N")[..12];
}
