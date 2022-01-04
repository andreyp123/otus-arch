using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BillingSvc.Repository
{
    public class AccountRepositoryHealthCheck : IHealthCheck
    {
        public const string NAME = "ShopDb";

        private string? _connectionString;

        public AccountRepositoryHealthCheck(AccountRepositoryConfig config)
        {
            _connectionString = config.ConnectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var postgresCheck = new NpgSqlHealthCheck(_connectionString, "SELECT COUNT(*) from \"Accounts\"");
            return await postgresCheck.CheckHealthAsync(context, cancellationToken);
        }
    }
}
