using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CarSvc.Dal;

public class CarDalHealthCheck : IHealthCheck
{
    public const string NAME = "DB";

    private NpgSqlHealthCheck _check;

    public CarDalHealthCheck(CarDalConfig config)
    {
        _check = new NpgSqlHealthCheck(config.ConnectionString, "select * from car_svc.cars");
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var lts = CancellationTokenSource.CreateLinkedTokenSource(
            new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token,
            cancellationToken);
        
        return await _check.CheckHealthAsync(context, lts.Token);
    }
}