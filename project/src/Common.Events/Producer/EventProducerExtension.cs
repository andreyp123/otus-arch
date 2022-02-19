using Microsoft.Extensions.DependencyInjection;

namespace Common.Events.Producer;

public static class EventProducerExtension
{
    public static IServiceCollection AddEventProducer(this IServiceCollection services)
    {
        services.AddSingleton<EventProducerConfig>();
        services.AddSingleton<IEventProducer, EventProducer>();

        services.AddHealthChecks()
            .AddCheck<EventProducerHealthCheck>(EventProducerHealthCheck.NAME);
        
        return services;
    }
}