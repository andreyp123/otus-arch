using Microsoft.Extensions.Configuration;

namespace NotificationSvc.Api.KafkaConsumer;

public class KafkaConsumerConfig
{
    public string BootstrapServers { get; set; }

    public KafkaConsumerConfig(IConfiguration configuration)
    {
        configuration.GetSection(nameof(KafkaConsumer)).Bind(this);
    }
}