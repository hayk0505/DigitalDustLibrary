using DigitalDustLibrary.Api.Data;
using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalDustLibrary.Api.Tests;

public static class UserTestHelpers
{
    public const string Password = "password";

    // Direct DB insert, not a real signup flow (there isn't one — users are
    // only ever created via Author Applications approval). Gives tests a
    // throwaway user to mutate without touching the three shared seeded
    // accounts every other test file in ApiCollection logs in as.
    public static async Task<ApplicationUser> CreateUserAsync(
        ApiFactory factory, string name, string email, Role role, bool isActive = true)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var handle = await SlugGenerator.GenerateUniqueAsync(name, h => db.Users.AnyAsync(u => u.Handle == h));
        var hasher = new PasswordHasher<ApplicationUser>();
        var user = new ApplicationUser { Name = name, Handle = handle, Email = email, Role = role, IsActive = isActive, PasswordHash = "" };
        user.PasswordHash = hasher.HashPassword(user, Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public static string UniqueSuffix() => Guid.NewGuid().ToString("N")[..12];
}
