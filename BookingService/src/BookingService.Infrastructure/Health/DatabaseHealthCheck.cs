using System.Data;
using BookingService.Infrastructure.Connection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookingService.Infrastructure.Health;

public class DatabaseHealthCheck(DbConnectionFactory dbConnectionFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using IDbConnection connection = dbConnectionFactory.CreateConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            
            await Task.Run(() => command.ExecuteScalar(), cancellationToken);

            return HealthCheckResult.Healthy("Database is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is unhealthy.", ex);
        }
    }
}