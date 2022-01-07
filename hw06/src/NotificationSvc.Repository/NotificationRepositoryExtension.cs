using Microsoft.EntityFrameworkCore;
using NotificationSvc.Repository.Model;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationSvc.Repository
{
    public static class NotificationRepositoryExtension
    {
        public static IServiceCollection AddNotificationRepository(this IServiceCollection services)
        {
            services.AddSingleton<NotificationRepositoryConfig>();
            services.AddDbContext<NotificationDbContext>(
                (provider, builder) =>
                {
                    var repoConfig = provider.GetRequiredService<NotificationRepositoryConfig>();
                    builder.UseNpgsql(repoConfig.ConnectionString);
                },
                contextLifetime: ServiceLifetime.Scoped
            );
            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddHealthChecks()
                .AddCheck<NotificationRepositoryHealthCheck>(NotificationRepositoryHealthCheck.NAME);

            return services;
        }
    }
}
