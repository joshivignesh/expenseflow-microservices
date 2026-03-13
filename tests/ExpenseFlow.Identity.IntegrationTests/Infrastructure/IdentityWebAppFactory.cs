using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using ExpenseFlow.Identity.Infrastructure.Persistence;

namespace ExpenseFlow.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the full Identity API in-process with a real SQL Server container.
/// Shared across all tests in the collection via IAsyncLifetime.
///
/// Model assignment:
///   - Container lifecycle management  → Haiku (heartbeat 💓)
///   - Test orchestration / assertions → Sonnet (brain 🧠)
/// </summary>
public sealed class IdentityWebAppFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test_P@ssw0rd123!")
        .Build();

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration from the app
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            // Replace with the Testcontainer connection string
            services.AddDbContext<IdentityDbContext>(opts =>
                opts.UseSqlServer(_sqlContainer.GetConnectionString()));

            // Override JWT settings for tests
            builder.UseSetting("JwtSettings:SecretKey",  "test-only-secret-key-min-32-chars!!");
            builder.UseSetting("JwtSettings:Issuer",     "ExpenseFlow.Identity");
            builder.UseSetting("JwtSettings:Audience",   "ExpenseFlow.Client");
            builder.UseSetting("JwtSettings:ExpiryMinutes", "15");

            // Apply migrations against the test container
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.Database.Migrate();
        });
    }

    public new async Task DisposeAsync()
    {
        await _sqlContainer.StopAsync();
    }
}
