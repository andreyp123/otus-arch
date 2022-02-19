using Confluent.Kafka;
using HealthChecks.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Events.Producer;

public class EventProducerHealthCheck : IHealthCheck
{
    public const string NAME = "Kafka";

    private readonly KafkaHealthCheck _check;

    public EventProducerHealthCheck(EventProducerConfig config)
    {
        _check = new KafkaHealthCheck(new ProducerConfig { BootstrapServers = config.BootstrapServers }, null);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var lts = CancellationTokenSource.CreateLinkedTokenSource(
            new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token,
            cancellationToken);
        
        return await _check.CheckHealthAsync(context, lts.Token);
    }
}