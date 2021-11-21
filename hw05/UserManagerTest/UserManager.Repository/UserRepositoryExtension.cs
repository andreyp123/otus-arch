using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManager.Repository.Model;

namespace UserManager.Repository
{
    public static class UserRepositoryExtension
    {
        public static IServiceCollection AddUserRepository(
            this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddSingleton<UserRepositoryConfig>();
            services.AddDbContext<UserDbContext>(
                (provider, builder) =>
                {
                    var repoConfig = provider.GetRequiredService<UserRepositoryConfig>();
                    builder.UseNpgsql(repoConfig.ConnectionString);
                },
                contextLifetime: ServiceLifetime.Scoped
            );
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddHealthChecks()
                .AddCheck<UserRepositoryHealthCheck>("UserDb");

            return services;
        }
    }
}
