using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Helpers;
using Common.Model.NotificationSvc;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OrderSvc.Api.KafkaProducer;

public class KafkaProducer : IKafkaProducer
{
    private readonly KafkaProducerConfig _config;
    private readonly ILogger<KafkaProducer> _logger;
    private readonly IProducer<Null, string> _producer;

    public KafkaProducer(ILogger<KafkaProducer> logger, KafkaProducerConfig config)
    {
        _logger = logger;
        _config = config;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _config.BootstrapServers
        };
        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task SendAsync(string topic, NotificationMessage message, CancellationToken ct = default)
    {
        _logger.LogInformation($"Sending message... Topic: {topic}. Message: {JsonSerializer.Serialize(message)}");
        try
        {
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = JsonHelper.Serialize(message) }, ct);
            _logger.LogInformation($"Sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
        }
    }
}