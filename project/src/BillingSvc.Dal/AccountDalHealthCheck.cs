using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BillingSvc.Dal
{
    public class AccountDalHealthCheck : IHealthCheck
    {
        public const string NAME = "DB";

        private NpgSqlHealthCheck _check;

        public AccountDalHealthCheck(AccountDalConfig config)
        {
            _check = new NpgSqlHealthCheck(config.ConnectionString, "select count(*) from billing_svc.accounts");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var lts = CancellationTokenSource.CreateLinkedTokenSource(
                new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token,
                cancellationToken);
            
            return await _check.CheckHealthAsync(context, lts.Token);
        }
    }
}
