using Microsoft.EntityFrameworkCore;
using BillingSvc.Repository.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BillingSvc.Repository
{
    public static class AccountRepositoryExtension
    {
        public static IServiceCollection AddAcocuntRepository(this IServiceCollection services)
        {
            services.AddSingleton<AccountRepositoryConfig>();
            services.AddDbContext<AccountDbContext>(
                (provider, builder) =>
                {
                    var repoConfig = provider.GetRequiredService<AccountRepositoryConfig>();
                    builder.UseNpgsql(repoConfig.ConnectionString);
                },
                contextLifetime: ServiceLifetime.Scoped
            );
            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddHealthChecks()
                .AddCheck<AccountRepositoryHealthCheck>(AccountRepositoryHealthCheck.NAME);

            return services;
        }
    }
}
