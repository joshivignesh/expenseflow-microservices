using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using ExpenseFlow.Expense.Infrastructure.Persistence;

namespace ExpenseFlow.Expense.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the full Expense API in-process with a real SQL Server container.
/// Same pattern as IdentityApiFactory — container starts once per collection.
/// </summary>
public sealed class ExpenseApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test_P@ssw0rd123!")
        .Build();

    public string ConnectionString => _sqlContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _sqlContainer.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ExpenseDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<ExpenseDbContext>(opts =>
                opts.UseSqlServer(ConnectionString));

            // Replace IDbConnection (Dapper) to point at the test container
            var dbConnDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(System.Data.IDbConnection));
            if (dbConnDescriptor is not null)
                services.Remove(dbConnDescriptor);

            services.AddTransient<System.Data.IDbConnection>(_ =>
                new Microsoft.Data.SqlClient.SqlConnection(ConnectionString));
        });

        builder.UseSetting("JwtSettings:SecretKey",  "test-secret-key-min-32-chars-long!!");
        builder.UseSetting("JwtSettings:Issuer",     "ExpenseFlow.Identity");
        builder.UseSetting("JwtSettings:Audience",   "ExpenseFlow.Client");
    }

    /// <summary>
    /// Returns an HttpClient with a pre-attached Bearer JWT for the given userId and role.
    /// Generates a real signed token using the test secret — no mocking required.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(
        Guid?   userId = null,
        string  role   = "Employee")
    {
        var id     = userId ?? Guid.NewGuid();
        var token  = JwtTestHelper.GenerateToken(id, role);
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
