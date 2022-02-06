using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NotificationSvc.Dal
{
    public class NotificationDalHealthCheck : IHealthCheck
    {
        public const string NAME = "DB";

        private string? _connectionString;

        public NotificationDalHealthCheck(NotificationDalConfig config)
        {
            _connectionString = config.ConnectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var postgresCheck = new NpgSqlHealthCheck(_connectionString, "select count(*) from notifications");
            return await postgresCheck.CheckHealthAsync(context, cancellationToken);
        }
    }
}
