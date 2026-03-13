using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using ExpenseFlow.Identity.Infrastructure.Persistence;

namespace ExpenseFlow.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the full Identity API in-process using WebApplicationFactory.
/// Replaces the real SQL Server with a Testcontainers SQL Server instance
/// so tests run against a real DB engine with zero infra setup.
///
/// Lifecycle:
///   - Container starts once per test collection (IAsyncLifetime)
///   - Schema is migrated fresh before the first test
///   - Container is torn down after the last test in the collection
/// </summary>
public sealed class IdentityApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test_P@ssw0rd123!")
        .Build();

    public string ConnectionString => _sqlContainer.GetConnectionString();

    // Called once before any test in the collection runs
    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        // Apply migrations against the fresh container database
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.Database.MigrateAsync();
    }

    // Called once after all tests in the collection finish
    public new async Task DisposeAsync()
    {
        await _sqlContainer.StopAsync();
        await base.DisposeAsync();
    }

    // Override configuration so the API uses the container's connection string
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Register with the Testcontainer connection string
            services.AddDbContext<IdentityDbContext>(opts =>
                opts.UseSqlServer(ConnectionString));
        });

        // Override JWT settings so token validation works in tests
        builder.UseSetting("JwtSettings:SecretKey",  "test-secret-key-min-32-chars-long!!");
        builder.UseSetting("JwtSettings:Issuer",     "ExpenseFlow.Identity");
        builder.UseSetting("JwtSettings:Audience",   "ExpenseFlow.Client");
        builder.UseSetting("JwtSettings:ExpiryMinutes", "15");
    }
}
