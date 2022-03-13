using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserSvc.Dal.Model;
using UserSvc.Dal.Repositories;

namespace UserSvc.Dal;

public static class UserDalExtension
{
    public static IServiceCollection AddUserDal(
        this IServiceCollection services)
    {
        services.AddSingleton<UserDalConfig>();
        services.AddDbContextFactory<UserDbContext>(
            (provider, builder) =>
            {
                var repoConfig = provider.GetRequiredService<UserDalConfig>();
                builder.SetUpUserDbContext(repoConfig.ConnectionString);
            },
            lifetime: ServiceLifetime.Singleton);
        // services.AddDbContext<UserDbContext>(
        //     (provider, builder) =>
        //     {
        //         var repoConfig = provider.GetRequiredService<UserDalConfig>();
        //         builder.SetUpUserDbContext(repoConfig.ConnectionString);
        //     },
        //     contextLifetime: ServiceLifetime.Scoped
        // );
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddHealthChecks()
            .AddCheck<UserDalHealthCheck>(UserDalHealthCheck.NAME);

        return services;
    }

    public static DbContextOptionsBuilder SetUpUserDbContext(
        this DbContextOptionsBuilder builder,
        string connectionString)
    {
        builder
            .EnableDetailedErrors()
            .UseNpgsql(
                connectionString,
                b => b.MigrationsHistoryTable("__ef_migration_history", UserDbContext.SCHEMA))
            .UseSnakeCaseNamingConvention();

        return builder;
    }
}
