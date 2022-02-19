using System;
using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace RentSvc.Dal
{
    public class RentDalHealthCheck : IHealthCheck
    {
        public const string NAME = "DB";
        
        private NpgSqlHealthCheck _check;

        public RentDalHealthCheck(RentDalConfig config)
        {
            _check = new NpgSqlHealthCheck(config.ConnectionString, "select count(*) from rent_svc.rents");
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
}
