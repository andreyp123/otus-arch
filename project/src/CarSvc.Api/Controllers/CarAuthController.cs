using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CarSvc.Dal.Repositories;
using Common;
using Common.Authentication;
using Common.Helpers;
using Common.Model.CarSvc;
using Common.Model.UserSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CarSvc.Api.Controllers;

[ApiController]
[Route("auth")]
public class CarAuthController : ControllerBase
{
    private readonly ILogger<CarAuthController> _logger;
    private readonly ICarRepository _repository;
    private TokenConfig _tokenConfig;

    public CarAuthController(ILogger<CarAuthController> logger, ICarRepository repository, TokenConfig tokenConfig)
    {
        _logger = logger;
        _repository = repository;
        _tokenConfig = tokenConfig;
    }

    [HttpGet("token")]
    [AllowAnonymous]
    public async Task<UserSecurityToken> Login(string carId, string carApiKey)
    {
        var car = await _repository.GetCarAsync(carId, HttpContext.RequestAborted);
        if (car == null || car.ApiKeyHash != Hasher.CalculateHash(carApiKey))
        {
            throw new CrashException("Incorrect id or apikey.");
        }

        var token = IssueToken(car);
        return token;
    }

    [HttpGet("check")]
    [Authorize]
    public Task CheckAuth()
    {
        _ = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return Task.CompletedTask;
    }

    private UserSecurityToken IssueToken(Car car)
    {
        var now = DateTime.UtcNow;

        var jwt = new JwtSecurityToken(
            issuer: _tokenConfig.Issuer,
            audience: _tokenConfig.Audience,
            notBefore: now,
            expires: now.Add(TimeSpan.FromSeconds(_tokenConfig.LifetimeSec)),
            claims: new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, car.CarId),
                new(ClaimTypes.Name, $"{car.Brand} {car.Model}"),
            },
            signingCredentials: new SigningCredentials(_tokenConfig.SigningKey, SecurityAlgorithms.HmacSha256));

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new UserSecurityToken {AccessToken = accessToken};
    }
}