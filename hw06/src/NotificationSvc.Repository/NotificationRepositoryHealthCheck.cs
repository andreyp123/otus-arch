using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NotificationSvc.Repository
{
    public class NotificationRepositoryHealthCheck : IHealthCheck
    {
        public const string NAME = "ShopDb";

        private string? _connectionString;

        public NotificationRepositoryHealthCheck(NotificationRepositoryConfig config)
        {
            _connectionString = config.ConnectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var postgresCheck = new NpgSqlHealthCheck(_connectionString, "SELECT COUNT(*) from \"Notifications\"");
            return await postgresCheck.CheckHealthAsync(context, cancellationToken);
        }
    }
}
