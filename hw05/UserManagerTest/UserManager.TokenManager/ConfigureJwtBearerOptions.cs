using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace UserManager.TokenManager
{
    public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly TokenManagerConfig _tokenConfig;

        public ConfigureJwtBearerOptions(TokenManagerConfig tokenConfig)
        {
            _tokenConfig = tokenConfig;
        }

        public void Configure(string name, JwtBearerOptions options)
        {
            if (name == JwtBearerDefaults.AuthenticationScheme)
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = _tokenConfig.ValidateIssuer,
                    ValidIssuer = _tokenConfig.Issuer,
                    ValidateAudience = _tokenConfig.ValidateAudience,
                    ValidAudience = _tokenConfig.Audience,
                    ValidateLifetime = _tokenConfig.ValidateLifetime,
                    IssuerSigningKey = _tokenConfig.SigningKey,
                    ValidateIssuerSigningKey = _tokenConfig.ValidateSigningKey,
                };
            }
        }

        public void Configure(JwtBearerOptions options)
        {
            Configure(string.Empty, options);
        }
    }
}
