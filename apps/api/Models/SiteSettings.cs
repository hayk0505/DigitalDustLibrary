namespace DigitalDustLibrary.Api.Models;

/// <summary>
/// Singleton — exactly one row ever exists, seeded by DbSeeder on startup.
/// GET/PATCH /api/settings operate on that one row directly; there's no id
/// in the route because there's nothing to disambiguate.
/// </summary>
public class SiteSettings
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string SiteTitle { get; set; } = "Digital Dust Library";
    public string Tagline { get; set; } = "";
    public string DefaultMetaDescription { get; set; } = "";
    public string LinkedInUrl { get; set; } = "";
    public string XUrl { get; set; } = "";
}
