using Microsoft.Extensions.DependencyInjection;

namespace RentSvc.Api.Service;

public static class RentServiceExtension
{
    public static IServiceCollection AddRentService(this IServiceCollection services)
    {
        services.AddSingleton<IRentService, RentService>();
        return services;
    }
}