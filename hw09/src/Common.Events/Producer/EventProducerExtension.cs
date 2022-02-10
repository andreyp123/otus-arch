using Microsoft.Extensions.DependencyInjection;

namespace Common.Events.Producer;

public static class EventProducerExtension
{
    public static IServiceCollection AddEventProducer(this IServiceCollection services)
    {
        services.AddSingleton<EventProducerConfig>();
        services.AddScoped<IEventProducer, EventProducer>();
        return services;
    }
}