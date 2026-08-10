using DigitalDustLibrary.Api.Models;
using DigitalDustLibrary.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Data;

/// <summary>
/// Seeds the same three accounts as apps/admin/mocks/fixtures/users.ts
/// (author@dd.local / editor@dd.local / owner@dd.local, all password "password")
/// so switching VITE_ENABLE_MOCKS off and pointing at this API is a true
/// drop-in for local dev/demo — same logins work either way. Dev/demo only:
/// don't run this against a real production database with real users.
/// Also seeds the one SiteSettings row every deployment needs to exist.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            var hasher = new PasswordHasher<ApplicationUser>();
            async Task<ApplicationUser> Make(string name, string email, Role role)
            {
                var handle = await SlugGenerator.GenerateUniqueAsync(name, h => db.Users.AnyAsync(u => u.Handle == h));
                var u = new ApplicationUser { Name = name, Handle = handle, Email = email, Role = role, PasswordHash = "" };
                u.PasswordHash = hasher.HashPassword(u, "password");
                return u;
            }

            db.Users.Add(await Make("Alex Rivera", "author@dd.local", Role.Author));
            db.Users.Add(await Make("Jordan Blake", "editor@dd.local", Role.Editor));
            db.Users.Add(await Make("Hayk Baroyan", "owner@dd.local", Role.Owner));
        }

        if (!await db.SiteSettings.AnyAsync())
        {
            db.SiteSettings.Add(new SiteSettings());
        }

        await db.SaveChangesAsync();
    }
}
