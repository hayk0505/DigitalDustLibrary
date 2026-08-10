using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalDustLibrary.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings").WithTags("Settings").RequireAuthorization("OwnerOnly");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var settings = await db.SiteSettings.SingleAsync();
            return Results.Ok(settings.ToDto());
        });

        // PATCH always overwrites every field, unlike Categories/Users/Posts'
        // if-not-null partial-update PATCH endpoints. This form always submits
        // its complete current state and must be able to clear a field to
        // empty (e.g. removing a social link) — a null-means-"leave it alone"
        // convention can't express that. See
        // docs/superpowers/specs/2026-08-10-site-settings-design.md.
        group.MapPatch("/", async (UpdateSiteSettingsRequest request, AppDbContext db) =>
        {
            var settings = await db.SiteSettings.SingleAsync();
            settings.SiteTitle = request.SiteTitle;
            settings.Tagline = request.Tagline;
            settings.DefaultMetaDescription = request.DefaultMetaDescription;
            settings.LinkedInUrl = request.LinkedInUrl;
            settings.XUrl = request.XUrl;

            await db.SaveChangesAsync();
            return Results.Ok(settings.ToDto());
        });
    }
}
