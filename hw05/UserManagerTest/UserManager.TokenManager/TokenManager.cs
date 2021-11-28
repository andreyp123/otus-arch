using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserManager.Common.Model;

namespace UserManager.TokenManager
{
    public class TokenManager : ITokenManager
    {
        private TokenManagerConfig _config;

        public TokenManager(TokenManagerConfig config)
        {
            _config = config;
        }

        public string IssueToken(User user)
        {
            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: now,
                expires: now.Add(TimeSpan.FromSeconds(_config.LifetimeSec)),
                claims: GetClaims(user),
                signingCredentials: new SigningCredentials(_config.SigningKey, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private IEnumerable<Claim> GetClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Username),
            };

            claims.AddRange(user.Roles.Select(role =>
                new Claim(ClaimTypes.Role, role)));

            return claims;
        }
    }
}
