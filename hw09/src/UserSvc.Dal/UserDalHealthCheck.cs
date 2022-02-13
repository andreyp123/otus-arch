using System;
using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace UserSvc.Dal;

public class UserDalHealthCheck : IHealthCheck
{
    public const string NAME = "DB";

    private NpgSqlHealthCheck _check;

    public UserDalHealthCheck(UserDalConfig config)
    {
        _check = new NpgSqlHealthCheck(config.ConnectionString, "select count(*) from user_svc.users");
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