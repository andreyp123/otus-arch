using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace UserSvc.Dal;

public class UserDalHealthCheck : IHealthCheck
{
    public const string NAME = "DB";

    private string _connectionString;

    public UserDalHealthCheck(UserDalConfig config)
    {
        _connectionString = config.ConnectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var postgresCheck = new NpgSqlHealthCheck(_connectionString, "select count(*) from users");
        return await postgresCheck.CheckHealthAsync(context, cancellationToken);
    }
}