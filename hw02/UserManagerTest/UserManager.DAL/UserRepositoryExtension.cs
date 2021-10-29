using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManager.DAL.Model;

namespace UserManager.DAL
{
    public static class UserRepositoryExtension
    {
        public static IServiceCollection AddUserRepository(
            this IServiceCollection services)
        {
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

            return services;
        }
    }
}
