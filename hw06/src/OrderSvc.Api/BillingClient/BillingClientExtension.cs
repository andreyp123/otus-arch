using Microsoft.Extensions.DependencyInjection;

namespace OrderSvc.Api.BillingClient;

public static class BillingClientExtension
{
    public static IServiceCollection AddBillingClient(this IServiceCollection services)
    {
        services.AddSingleton<BillingClientConfig>();
        services.AddScoped<IBillingClient, BillingClient>();
        return services;
    }
}