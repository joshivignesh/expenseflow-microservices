using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using ExpenseFlow.Expense.Infrastructure.Persistence;

namespace ExpenseFlow.Expense.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the full Expense API in-process with a real SQL Server Testcontainer.
/// </summary>
public sealed class ExpenseWebAppFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test_P@ssw0rd123!")
        .Build();

    public const string TestJwtSecret = "test-only-secret-key-min-32-chars!!";

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Swap real DbContext for test container
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ExpenseDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<ExpenseDbContext>(opts =>
                opts.UseSqlServer(_sqlContainer.GetConnectionString()));

            builder.UseSetting("JwtSettings:SecretKey",  TestJwtSecret);
            builder.UseSetting("JwtSettings:Issuer",     "ExpenseFlow.Identity");
            builder.UseSetting("JwtSettings:Audience",   "ExpenseFlow.Client");

            // Apply migrations
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
            db.Database.Migrate();
        });
    }

    public new async Task DisposeAsync()
    {
        await _sqlContainer.StopAsync();
    }
}
