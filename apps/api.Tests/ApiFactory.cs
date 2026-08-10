using DigitalDustLibrary.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace DigitalDustLibrary.Api.Tests;

// Boots the real app against a real, ephemeral Postgres 18 container (not EF
// Core InMemory) so this suite exercises the same engine the app has already
// been manually verified against (see CLAUDE.md's "apps/api status" note),
// including the unique index on Categories.Slug. One instance of this class
// is shared across the whole test run via ApiCollection, not spun up per test
// or per class — starting a fresh Postgres container per test would be slow.
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18")
        .WithDatabase("digitaldustlibrary_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Forces the same startup path Program.cs runs in Development: applies
        // pending EF Core migrations and seeds the three demo accounts.
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _postgres.GetConnectionString(),
            });
        });
    }

    async Task IAsyncLifetime.InitializeAsync() => await _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
