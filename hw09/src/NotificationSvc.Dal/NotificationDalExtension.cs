using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationSvc.Dal.Model;
using NotificationSvc.Dal.Repositories;

namespace NotificationSvc.Dal;

public static class NotificationDalExtension
{
    public static IServiceCollection AddNotificationDal(this IServiceCollection services)
    {
        services.AddSingleton<NotificationDalConfig>();
        services.AddDbContextFactory<NotificationDbContext>(
            (provider, builder) =>
            {
                var repoConfig = provider.GetRequiredService<NotificationDalConfig>();
                builder.SetUpNotificationDbContext(repoConfig.ConnectionString);
            },
            lifetime: ServiceLifetime.Singleton);
        services.AddSingleton<INotificationRepository, NotificationRepository>();

        services.AddHealthChecks()
            .AddCheck<NotificationDalHealthCheck>(NotificationDalHealthCheck.NAME);

        return services;
    }

    public static DbContextOptionsBuilder SetUpNotificationDbContext(
        this DbContextOptionsBuilder builder,
        string connectionString)
    {
        builder
            .EnableDetailedErrors()
            .UseNpgsql(
                connectionString,
                b => b.MigrationsHistoryTable("__ef_migration_history", NotificationDbContext.SCHEMA))
            .UseSnakeCaseNamingConvention();

        return builder;
    }
}
