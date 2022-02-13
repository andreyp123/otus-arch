using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NotificationSvc.Dal
{
    public class NotificationDalHealthCheck : IHealthCheck
    {
        public const string NAME = "DB";

        private NpgSqlHealthCheck _check;

        public NotificationDalHealthCheck(NotificationDalConfig config)
        {
            _check = new NpgSqlHealthCheck(config.ConnectionString, "select count(*) from notification_svc.notifications");
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
