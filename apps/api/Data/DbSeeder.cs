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

    /// <summary>
    /// Production bootstrap — creates exactly one Owner account if the Users
    /// table is completely empty and both email/password are provided (via
    /// Bootstrap:OwnerEmail / Bootstrap:OwnerPassword config, i.e. the
    /// BOOTSTRAP__OWNEREMAIL / BOOTSTRAP__OWNERPASSWORD env vars on the
    /// droplet). Unlike SeedAsync above — which is Development-only and
    /// seeds fixed, publicly-known demo credentials that must never touch a
    /// real database — this is meant to run in every environment, since
    /// there's no public sign-up and the author-application approval flow
    /// itself requires an existing Owner, so something has to create the
    /// very first real account. Safe to leave configured indefinitely: it's
    /// a permanent no-op the instant any user exists. Still worth removing
    /// the BOOTSTRAP_* values from the droplet's .env after the first login
    /// though, just so a real password isn't sitting in a plaintext file
    /// longer than it needs to be.
    /// </summary>
    public static async Task BootstrapOwnerAsync(AppDbContext db, string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        if (await db.Users.AnyAsync())
            return;

        var hasher = new PasswordHasher<ApplicationUser>();
        var handle = await SlugGenerator.GenerateUniqueAsync("Owner", h => db.Users.AnyAsync(u => u.Handle == h));
        var owner = new ApplicationUser { Name = "Owner", Handle = handle, Email = email, Role = Role.Owner, PasswordHash = "" };
        owner.PasswordHash = hasher.HashPassword(owner, password);
        db.Users.Add(owner);

        await db.SaveChangesAsync();
    }
}
