using Common;
using Common.Authentication;
using Common.Helpers;
using Common.Model.UserSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UserSvc.Repository;

namespace UserSvc.Api.Controllers
{
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly ILogger<UserAuthController> _logger;
        private readonly IUserRepository _repository;
        private TokenConfig _tokenConfig;

        public UserAuthController(ILogger<UserAuthController> logger, IUserRepository repository, TokenConfig tokenConfig)
        {
            _logger = logger;
            _repository = repository;
            _tokenConfig = tokenConfig;
        }

        [HttpGet("auth/token")]
        [AllowAnonymous]
        public async Task<UserSecurityToken> Login(string username, string password)
        {
            var user = await _repository.GetUserByNameAsync(username, HttpContext.RequestAborted);
            if (user == null || user.PasswordHash != Hasher.CalculateHash(password))
            {
                throw new EShopException("Incorrect username or password.");
            }

            var token = IssueToken(user);
            return token;
        }

        [HttpGet("auth/check")]
        [Authorize]
        public async Task CheckAuth()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            await Task.FromResult(0);
        }

        private UserSecurityToken IssueToken(User user)
        {
            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: _tokenConfig.Issuer,
                audience: _tokenConfig.Audience,
                notBefore: now,
                expires: now.Add(TimeSpan.FromSeconds(_tokenConfig.LifetimeSec)),
                claims: GetClaims(user),
                signingCredentials: new SigningCredentials(_tokenConfig.SigningKey, SecurityAlgorithms.HmacSha256));

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            return new UserSecurityToken { AccessToken = accessToken };
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
