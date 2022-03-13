using Microsoft.Extensions.DependencyInjection;

namespace RentSvc.Api.Cache;

public static class CacheManagerExtension
{
    public static IServiceCollection AddCacheManager(this IServiceCollection services)
    {
        services.AddSingleton<CacheManagerConfig>();
        services.AddSingleton<ICacheManager, CacheManager>();
        return services;
    }
}