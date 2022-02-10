using Microsoft.Extensions.Configuration;

namespace Common.Events.Consumer;

public class EventConsumerConfig
{
    public string BootstrapServers { get; set; }
    public string GroupId { get; set; }

    public EventConsumerConfig(IConfiguration configuration)
    {
        configuration.GetSection(nameof(EventConsumer)).Bind(this);
    }
}