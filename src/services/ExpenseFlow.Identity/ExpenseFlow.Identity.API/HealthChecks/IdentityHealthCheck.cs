using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ExpenseFlow.Identity.Infrastructure.Persistence;

namespace ExpenseFlow.Identity.API.HealthChecks;

/// <summary>
/// Verifies the Identity database is reachable.
/// Exposed at GET /health — polled by YARP and docker-compose healthcheck.
/// </summary>
public sealed class IdentityHealthCheck : IHealthCheck
{
    private readonly IdentityDbContext _db;

    public IdentityHealthCheck(IdentityDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1", ct);
            return HealthCheckResult.Healthy("Identity database is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Identity database is unreachable.", ex);
        }
    }
}
