using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace UserSvc.Repository
{
    public class UserRepositoryHealthCheck : IHealthCheck
    {
        public const string NAME = "ShopDb";
        
        private string _connectionString;

        public UserRepositoryHealthCheck(UserRepositoryConfig config)
        {
            _connectionString = config.ConnectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var postgresCheck = new NpgSqlHealthCheck(_connectionString, "SELECT COUNT(*) from \"Users\"");
            return await postgresCheck.CheckHealthAsync(context, cancellationToken);
        }
    }
}
