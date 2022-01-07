using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace OrderSvc.Repository
{
    public class OrderRepositoryHealthCheck : IHealthCheck
    {
        public const string NAME = "ShopDb";
        
        private string _connectionString;

        public OrderRepositoryHealthCheck(OrderRepositoryConfig config)
        {
            _connectionString = config.ConnectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var postgresCheck = new NpgSqlHealthCheck(_connectionString, "SELECT COUNT(*) from \"Orders\"");
            return await postgresCheck.CheckHealthAsync(context, cancellationToken);
        }
    }
}
