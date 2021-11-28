using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UserManager.TokenManager
{
    public static class TokenAuthenticationExtension
    {
        public static IServiceCollection AddTokenAuthentication(
            this IServiceCollection services)
        {
            services.AddSingleton<TokenManagerConfig>();
            services.AddSingleton<ITokenManager, TokenManager>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions: null);

            services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

            return services;
        }
    }
}
