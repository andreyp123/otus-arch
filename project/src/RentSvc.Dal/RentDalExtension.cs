using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentSvc.Dal.Model;
using RentSvc.Dal.Repositories;

namespace RentSvc.Dal
{
    public static class RentDalExtension
    {
        public static IServiceCollection AddRentDal(
            this IServiceCollection services)
        {
            services.AddSingleton<RentDalConfig>();
            services.AddDbContextFactory<RentDbContext>(
                (provider, builder) =>
                {
                    var repoConfig = provider.GetRequiredService<RentDalConfig>();
                    builder.SetUpRentDbContext(repoConfig.ConnectionString);
                },
                lifetime: ServiceLifetime.Singleton);
            // services.AddDbContext<RentDbContext>(
            //     (provider, builder) =>
            //     {
            //         var repoConfig = provider.GetRequiredService<RentDalConfig>();
            //         builder.SetUpRentDbContext(repoConfig.ConnectionString);
            //     },
            //     contextLifetime: ServiceLifetime.Scoped
            // );
            services.AddSingleton<IRentRepository, RentRepository>();
            services.AddSingleton<IRequestRepository, RequestRepository>();

            services.AddHealthChecks()
                .AddCheck<RentDalHealthCheck>(RentDalHealthCheck.NAME);

            return services;
        }

        public static DbContextOptionsBuilder SetUpRentDbContext(
            this DbContextOptionsBuilder builder,
            string connectionString)
        {
            builder
                .EnableDetailedErrors()
                .UseNpgsql(
                    connectionString,
                    b => b.MigrationsHistoryTable("__ef_migration_history", RentDbContext.SCHEMA))
                .UseSnakeCaseNamingConvention();

            return builder;
        }
    }
}
