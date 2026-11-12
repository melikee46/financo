using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Finaco.Api.Infrastructure;

public sealed class DatabaseHealthCheck(FinacoDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Database connection is unavailable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Database connection check failed.", exception);
        }
    }
}

