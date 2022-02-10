using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CarSvc.Dal;

public class CarDalHealthCheck : IHealthCheck
{
    public const string NAME = "DB";

    private string? _connectionString;

    public CarDalHealthCheck(CarDalConfig config)
    {
        _connectionString = config.ConnectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var postgresCheck = new NpgSqlHealthCheck(_connectionString, "select count(*) from cars");
        return await postgresCheck.CheckHealthAsync(context, cancellationToken);
    }
}