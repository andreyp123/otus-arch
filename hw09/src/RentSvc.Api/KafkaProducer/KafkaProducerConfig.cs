using Microsoft.Extensions.Configuration;

namespace RentSvc.Api.KafkaProducer;

public class KafkaProducerConfig
{
    public string BootstrapServers { get; set; }

    public KafkaProducerConfig(IConfiguration configuration)
    {
        configuration.GetSection(nameof(KafkaProducer)).Bind(this);
    }
}