using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderSvc.Repository.Model;

namespace OrderSvc.Repository
{
    public static class OrderRepositoryExtension
    {
        public static IServiceCollection AddOrderRepository(
            this IServiceCollection services)
        {
            services.AddSingleton<OrderRepositoryConfig>();
            services.AddDbContext<OrderDbContext>(
                (provider, builder) =>
                {
                    var repoConfig = provider.GetRequiredService<OrderRepositoryConfig>();
                    builder.UseNpgsql(repoConfig.ConnectionString);
                },
                contextLifetime: ServiceLifetime.Scoped
            );
            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddHealthChecks()
                .AddCheck<OrderRepositoryHealthCheck>(OrderRepositoryHealthCheck.NAME);

            return services;
        }
    }
}
