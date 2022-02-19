using Microsoft.Extensions.DependencyInjection;

namespace Common.Events.Consumer;

public static class EventConsumerExtension
{
    public static IServiceCollection AddEventConsumer(this IServiceCollection services)
    {
        services.AddSingleton<EventConsumerConfig>();
        services.AddHostedService<EventConsumer>();
        return services;
    }
}