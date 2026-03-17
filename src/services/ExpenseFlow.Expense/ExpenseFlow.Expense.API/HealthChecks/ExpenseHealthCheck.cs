using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ExpenseFlow.Expense.Infrastructure.Persistence;

namespace ExpenseFlow.Expense.API.HealthChecks;

/// <summary>
/// Custom health check — verifies the Expense database is reachable.
/// Uses Haiku (💓) — lightweight heartbeat polling by YARP every 10s.
/// </summary>
public sealed class ExpenseHealthCheck : IHealthCheck
{
    private readonly ExpenseDbContext _db;

    public ExpenseHealthCheck(ExpenseDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1", ct);
            return HealthCheckResult.Healthy("Expense database is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Expense database is unreachable.", ex);
        }
    }
}
