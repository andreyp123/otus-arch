using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CarSvc.Dal.Model;
using CarSvc.Dal.Repositories;

namespace CarSvc.Dal;

public static class CarDalExtension
{
    public static IServiceCollection AddCarDal(this IServiceCollection services)
    {
        services.AddSingleton<CarDalConfig>();
        services.AddDbContextFactory<CarDbContext>(
            (provider, builder) =>
            {
                var repoConfig = provider.GetRequiredService<CarDalConfig>();
                builder.SetUpCarDbContext(repoConfig.ConnectionString);
            },
            lifetime: ServiceLifetime.Singleton);
        services.AddSingleton<ICarRepository, CarRepository>();

        services.AddHealthChecks()
            .AddCheck<CarDalHealthCheck>(CarDalHealthCheck.NAME);

        return services;
    }

    public static DbContextOptionsBuilder SetUpCarDbContext(
        this DbContextOptionsBuilder builder,
        string connectionString)
    {
        builder
            .EnableDetailedErrors()
            .UseNpgsql(
                connectionString,
                b => b.MigrationsHistoryTable("__ef_migration_history", CarDbContext.SCHEMA))
            .UseSnakeCaseNamingConvention();

        return builder;
    }
}
