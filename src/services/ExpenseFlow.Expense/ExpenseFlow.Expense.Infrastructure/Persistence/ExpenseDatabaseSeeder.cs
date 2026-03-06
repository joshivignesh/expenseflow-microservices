using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExpenseFlow.Expense.Infrastructure.Persistence;

/// <summary>
/// Runs at startup to apply any pending EF Core migrations.
/// Unlike the Identity seeder, no seed data is needed here —
/// expenses are created by users through the API.
/// </summary>
public sealed class ExpenseDatabaseSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpenseDatabaseSeeder> _logger;

    public ExpenseDatabaseSeeder(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpenseDatabaseSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope  = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<ExpenseDbContext>();

        _logger.LogInformation("[ExpenseService] Applying pending migrations…");
        await context.Database.MigrateAsync(ct);
        _logger.LogInformation("[ExpenseService] Database is up to date.");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
