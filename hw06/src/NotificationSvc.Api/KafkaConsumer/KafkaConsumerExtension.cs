using Microsoft.Extensions.DependencyInjection;

namespace NotificationSvc.Api.KafkaConsumer;

public static class KafkaConsumerExtension
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services)
    {
        services.AddSingleton<KafkaConsumerConfig>();
        services.AddHostedService<KafkaConsumer>();
        return services;
    }
}