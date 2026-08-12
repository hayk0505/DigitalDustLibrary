using System.Net;
using System.Net.Http.Json;
using DigitalDustLibrary.Api.Contracts;
using DigitalDustLibrary.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace DigitalDustLibrary.Api.Tests;

// Regression coverage for a real prod incident: GET/PATCH /api/settings 500'd
// because the SiteSettings singleton row was only ever created by
// DbSeeder.SeedAsync, which Program.cs only calls when IsDevelopment() — a
// real production boot (ASPNETCORE_ENVIRONMENT=Production, see
// docker-compose.prod.yml) never got that block, so the row never existed
// and SettingsEndpoints.cs's db.SiteSettings.SingleAsync() threw.
//
// ApiFactory (used by every other test in this project) can't reproduce this
// — it hardcodes UseEnvironment("Development") specifically so the shared
// suite gets fast, seeded demo accounts, which is exactly why this gap was
// invisible to the whole existing test suite. This factory instead boots
// Program.cs the way the real droplet does: Production environment, with EF
// Core migrations applied as a separate step against a standalone
// AppDbContext BEFORE the host starts (mirroring the deploy pipeline's baked
// -in `efbundle` step running before the API container itself starts — see
// docs/deployment.md) rather than via Program.cs's own Development-only
// auto-migrate block.
public class ProductionLikeApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18")
        .WithDatabase("digitaldustlibrary_prodboot_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _postgres.GetConnectionString(),
                // Mirrors how the real droplet creates its first real Owner
                // account (see DbSeeder.BootstrapOwnerAsync) — this is the
                // only account-creation path that runs outside Development,
                // so it's what this test logs in with via AuthHelper.
                ["Bootstrap:OwnerEmail"] = AuthHelper.OwnerEmail,
                ["Bootstrap:OwnerPassword"] = "password",
            });
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}

public class ProductionBootTests(ProductionLikeApiFactory factory) : IClassFixture<ProductionLikeApiFactory>
{
    [Fact]
    public async Task ProductionStartup_CreatesExactlyOneSiteSettingsRow()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var count = await db.SiteSettings.CountAsync();

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Get_SettingsAsOwner_OnFreshProductionBoot_ReturnsOkNot500()
    {
        var owner = await AuthHelper.LoginAsAsync(factory, AuthHelper.OwnerEmail);

        var response = await owner.GetAsync("/api/settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var settings = await response.Content.ReadFromJsonAsync<SiteSettingsDto>(AuthHelper.JsonOptions);
        Assert.NotEqual(Guid.Empty, settings!.Id);
    }
}
