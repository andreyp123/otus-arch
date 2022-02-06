using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BillingSvc.Dal
{
    public class AccountDalHealthCheck : IHealthCheck
    {
        public const string NAME = "DB";

        private string? _connectionString;

        public AccountDalHealthCheck(AccountDalConfig config)
        {
            _connectionString = config.ConnectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var postgresCheck = new NpgSqlHealthCheck(_connectionString, "select count(*) from accounts");
            return await postgresCheck.CheckHealthAsync(context, cancellationToken);
        }
    }
}
