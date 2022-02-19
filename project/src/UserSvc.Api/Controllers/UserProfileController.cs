using Common;
using Common.Helpers;
using Common.Model.UserSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UserSvc.Dal.Repositories;

namespace UserSvc.Api.Controllers;

[ApiController]
[Route("profile")]
public class UserProfileController : ControllerBase
{
    private readonly ILogger<UserProfileController> _logger;
    private readonly IUserRepository _repository;

    public UserProfileController(ILogger<UserProfileController> logger, IUserRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [HttpPost("")]
    [AllowAnonymous]
    public async Task CreateUserProfile([FromBody] UpdateUserProfileDto profile)
    {
        Guard.NotNull(profile, nameof(profile));
        Guard.NotNullOrEmpty(profile.Username, nameof(profile.Username));
        Guard.NotNullOrEmpty(profile.Password, nameof(profile.Password));

        await _repository.CreateUserAsync(
            new User
            {
                UserId = Generator.GenerateId(),
                Username = profile.Username,
                FullName = profile.FullName,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                DriverLicense = profile.DriverLicense,
                PasswordHash = Hasher.CalculateHash(profile.Password),
                Roles = new[] {UserRoles.Client}
            },
            HttpContext.RequestAborted);
    }

    [HttpGet("")]
    [Authorize]
    public async Task<UserProfileDto> GetUserProfile()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        User user = await _repository.GetUserAsync(userId, HttpContext.RequestAborted);
        return new UserProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DriverLicense = user.DriverLicense,
            Verified = user.Verified,
            Roles = user.Roles
        };
    }

    [HttpPut("")]
    [Authorize]
    public async Task UpdateUserProfile([FromBody] UpdateUserProfileDto profile)
    {
        Guard.NotNull(profile, nameof(profile));
        Guard.NotNullOrEmpty(profile.Username, nameof(profile.Username));

        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        await _repository.UpdateUserAsync(userId,
            new User
            {
                UserId = userId,
                Username = profile.Username,
                FullName = profile.FullName,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                DriverLicense = profile.DriverLicense,
                PasswordHash = !string.IsNullOrEmpty(profile.Password)
                    ? Hasher.CalculateHash(profile.Password)
                    : null
            },
            selfUpdate: true,
            HttpContext.RequestAborted);
    }
}