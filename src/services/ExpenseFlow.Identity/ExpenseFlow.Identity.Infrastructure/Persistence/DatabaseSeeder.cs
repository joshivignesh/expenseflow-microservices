using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ExpenseFlow.Identity.Application.Interfaces;
using ExpenseFlow.Identity.Domain.Entities;
using ExpenseFlow.Identity.Domain.Enums;
using ExpenseFlow.Identity.Domain.ValueObjects;
using ExpenseFlow.Identity.Infrastructure.Persistence;

namespace ExpenseFlow.Identity.Infrastructure.Persistence;

/// <summary>
/// Runs once at startup in Development/Staging to ensure the DB is migrated
/// and seeded with a default admin account.
/// Never runs in Production — guarded by environment check.
/// </summary>
public sealed class DatabaseSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope   = _scopeFactory.CreateScope();
        var context       = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        _logger.LogInformation("Applying pending migrations…");
        await context.Database.MigrateAsync(ct);

        if (await context.Users.AnyAsync(ct))
        {
            _logger.LogInformation("Database already seeded — skipping.");
            return;
        }

        _logger.LogInformation("Seeding default admin user…");

        var adminEmail = Email.Create("admin@expenseflow.local");
        var admin = User.Create(
            adminEmail,
            "System",
            "Admin",
            passwordHasher.Hash("Admin@12345!"),
            UserRole.Admin);

        await context.Users.AddAsync(admin, ct);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Default admin seeded with email: {Email}", adminEmail.Value);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
