using Microsoft.Extensions.Configuration;

namespace Common.Events.Producer;

public class EventProducerConfig
{
    public string BootstrapServers { get; set; }

    public EventProducerConfig(IConfiguration configuration)
    {
        configuration.GetSection(nameof(EventProducer)).Bind(this);
    }
}