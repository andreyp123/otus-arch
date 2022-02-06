using BillingSvc.Dal.Model;
using BillingSvc.Dal.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BillingSvc.Dal
{
    public static class AccountDalExtension
    {
        public static IServiceCollection AddAccountDal(this IServiceCollection services)
        {
            services.AddSingleton<AccountDalConfig>();
            services.AddDbContext<AccountDbContext>(
                (provider, builder) =>
                {
                    var repoConfig = provider.GetRequiredService<AccountDalConfig>();
                    builder.SetUpAccountDbContext(repoConfig.ConnectionString);
                },
                contextLifetime: ServiceLifetime.Scoped
            );
            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddHealthChecks()
                .AddCheck<AccountDalHealthCheck>(AccountDalHealthCheck.NAME);

            return services;
        }

        public static DbContextOptionsBuilder SetUpAccountDbContext(
            this DbContextOptionsBuilder builder,
            string connectionString)
        {
            builder
                .EnableDetailedErrors()
                .UseNpgsql(
                    connectionString,
                    b => b.MigrationsHistoryTable("__ef_migration_history", AccountDbContext.SCHEMA))
                .UseSnakeCaseNamingConvention();

            return builder;
        }
    }
}
