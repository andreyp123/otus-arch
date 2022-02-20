using Common.Helpers;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Common.Events.Producer;

public class EventProducer : IEventProducer
{
    private readonly EventProducerConfig _config;
    private readonly ILogger<EventProducer> _logger;
    private readonly IProducer<string, string> _producer;

    public EventProducer(ILogger<EventProducer> logger, EventProducerConfig config)
    {
        _logger = logger;
        _config = config;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _config.BootstrapServers
        };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }
    
    public async Task ProduceEventAsync<TEvent>(EventKey ek, TEvent ev, CancellationToken ct = default)
    {
        _logger.LogInformation($"Producing event {ek.EventType} to topic {ek.Topic}...");
        try
        {
            var evJson = JsonHelper.Serialize(ev);
            await _producer.ProduceAsync(ek.Topic, new Message<string, string> { Key = ek.EventType, Value = evJson }, ct);
            _logger.LogInformation($"Successfully produced event: {evJson}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing event");
        }
    }
}