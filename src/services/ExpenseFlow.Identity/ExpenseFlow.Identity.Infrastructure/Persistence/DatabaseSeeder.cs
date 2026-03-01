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
/// Runs once at startup in Development/Staging to seed an admin user.
/// Uses IHostedService so it executes after DI is built but before traffic is served.
/// Never seeds in Production — guarded by environment check.
/// </summary>
public sealed class DatabaseSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IServiceScopeFactory scopeFactory, ILogger<DatabaseSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope   = _scopeFactory.CreateAsyncScope();
        var context             = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var passwordHasher      = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        _logger.LogInformation("Applying pending EF Core migrations...");
        await context.Database.MigrateAsync(ct);

        if (await context.Users.AnyAsync(ct))
        {
            _logger.LogInformation("Database already seeded — skipping.");
            return;
        }

        _logger.LogInformation("Seeding admin user...");

        var adminEmail = Email.Create("admin@expenseflow.com");
        var admin      = User.Create(
            adminEmail,
            firstName:    "ExpenseFlow",
            lastName:     "Admin",
            passwordHash: passwordHasher.Hash("Admin@12345!"),
            role:         UserRole.Admin);

        await context.Users.AddAsync(admin, ct);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Admin user seeded successfully (ID: {UserId})", admin.Id);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
