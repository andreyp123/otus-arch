using Microsoft.Extensions.DependencyInjection;

namespace RentSvc.Api.KafkaProducer;

public static class KafkaProducerExtension
{
    public static IServiceCollection AddKafkaProducer(this IServiceCollection services)
    {
        services.AddSingleton<KafkaProducerConfig>();
        services.AddScoped<IKafkaProducer, KafkaProducer>();
        return services;
    }
}