using Microsoft.Extensions.DependencyInjection;
using RentSvc.Api.Cache;

namespace RentSvc.Api.Service;

public static class RentServiceExtension
{
    public static IServiceCollection AddRentService(this IServiceCollection services)
    {
        services.AddSingleton<IRentService, RentService>();
        return services;
    }
}