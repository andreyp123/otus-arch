using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace UserManager.TokenManager
{
    public class TokenManagerConfig
    {
        public string Issuer { get; set; } = "my-issuer";
        public string Audience { get; set; } = "my-audience";
        public int LifetimeSec { get; set; } = 1800; // 30min
        public string SigningSecret { get; set; } = "my-signing-secret"; // length >= 16

        public bool ValidateIssuer { get; set; } = true;
        public bool ValidateAudience { get; set; } = true;
        public bool ValidateLifetime { get; set; } = true;
        public bool ValidateSigningKey { get; set; } = true;

        public SymmetricSecurityKey SigningKey
        {
            get { return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SigningSecret)); }
        }
        
        public TokenManagerConfig(IConfiguration configuration)
        {
            //configuration.GetSection(nameof(TokenManager)).Bind(this);
        }
    }
}