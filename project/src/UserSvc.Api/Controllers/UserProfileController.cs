using Common;
using Common.Helpers;
using Common.Model.UserSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Events.Producer;
using Common.Tracing;
using UserSvc.Api.Extensions;
using UserSvc.Dal.Repositories;

namespace UserSvc.Api.Controllers;

[ApiController]
[Route("profile")]
public class UserProfileController : ControllerBase
{
    private readonly ILogger<UserProfileController> _logger;
    private readonly IUserRepository _repository;
    private readonly IEventProducer _eventProducer;
    private readonly ITracer _tracer;

    public UserProfileController(ILogger<UserProfileController> logger, IUserRepository repository,
        IEventProducer eventProducer, ITracer tracer)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
        _tracer = tracer;
    }

    [HttpPost("")]
    [AllowAnonymous]
    public async Task CreateUserProfile([FromBody] UpdateUserProfileDto profile)
    {
        Guard.NotNull(profile, nameof(profile));
        Guard.NotNullOrEmpty(profile.Username, nameof(profile.Username));
        Guard.NotNullOrEmpty(profile.Password, nameof(profile.Password));
        
        using var activity = _tracer.StartActivity("CreateUserProfile");

        var ct = HttpContext.RequestAborted;

        var createdUser = await _repository.CreateUserAsync(
            new User
            {
                UserId = Generator.GenerateId(),
                Username = profile.Username,
                FullName = profile.FullName,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                DriverLicense = profile.DriverLicense,
                PasswordHash = Hasher.CalculateHash(profile.Password),
                Roles = new[] {UserRoles.Client},
                Verified = false
            }, ct);
        
        _eventProducer.ProduceUserUpdatedWithNoWait(createdUser, _logger, _tracer.GetContext(activity));
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
        
        using var activity = _tracer.StartActivity("UpdateUserProfile");

        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        
        var ct = HttpContext.RequestAborted;

        var userBeforeUpdate = await _repository.GetUserAsync(userId, ct);
        
        bool userVerified = userBeforeUpdate.Verified &&
                            userBeforeUpdate.FullName == profile.FullName &&
                            userBeforeUpdate.DriverLicense == profile.DriverLicense;
        
        var updatedUser = await _repository.UpdateUserAsync(userId,
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
                    : null,
                Verified = userVerified
            },
            selfUpdate: true, ct);
        
        _eventProducer.ProduceUserUpdatedWithNoWait(updatedUser, _logger, _tracer.GetContext(activity));
    }
}