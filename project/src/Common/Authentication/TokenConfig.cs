using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Common.Authentication;

public class TokenConfig
{
    public string Issuer { get; set; } = "my-issuer";
    public string Audience { get; set; } = "my-audience";
    public int LifetimeSec { get; set; } = 18000; // 5h
    public string SigningSecret { get; set; } = "my-signing-secret"; // length >= 16

    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = false;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateSigningKey { get; set; } = true;

    public SymmetricSecurityKey SigningKey
    {
        get { return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SigningSecret)); }
    }

    public TokenConfig(IConfiguration configuration)
    {
        // todo: get from configuration
        //configuration.GetSection(nameof(TokenManager)).Bind(this);
    }
}