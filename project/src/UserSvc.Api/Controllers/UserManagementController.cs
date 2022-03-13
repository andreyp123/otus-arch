using Common;
using Common.Helpers;
using Common.Model;
using Common.Model.UserSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Common.Events.Producer;
using UserSvc.Api.Extensions;
using UserSvc.Api.Helpers;
using UserSvc.Dal.Repositories;

namespace UserSvc.Api.Controllers;

[ApiController]
[Route("users")]
public class UserManagementController : ControllerBase
{
    private readonly ILogger<UserManagementController> _logger;
    private readonly IUserRepository _repository;
    private readonly IEventProducer _eventProducer;

    public UserManagementController(ILogger<UserManagementController> logger, IUserRepository repository, IEventProducer eventProducer)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
    }

    [HttpPost("")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task<string> CreateUser([FromBody] UpdateUserDto user)
    {
        Guard.NotNull(user, nameof(user));
        Guard.NotNullOrEmpty(user.Username, nameof(user.Username));
        Guard.NotNullOrEmpty(user.Password, nameof(user.Password));

        var ct = HttpContext.RequestAborted;

        var createdUser = await _repository.CreateUserAsync(
            new User
            {
                UserId = Generator.GenerateId(),
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DriverLicense = user.DriverLicense,
                Verified = user.Verified,
                PasswordHash = Hasher.CalculateHash(user.Password),
                Roles = user.Roles
            }, ct);
        
        _eventProducer.ProduceUserUpdatedWithNoWait(createdUser, _logger);

        return createdUser.UserId;
    }

    [HttpGet("")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task<ListResult<UserDto>> GetUsers([FromQuery] int start, [FromQuery] int size)
    {
        (User[] users, int total) = await _repository.GetUsersAsync(start, size, HttpContext.RequestAborted);
        return new ListResult<UserDto>(
            users.Select(UserMapper.MapUserDto).ToArray(),
            total);
    }

    [HttpGet("{userId}")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task<UserDto> GetUser(string userId)
    {
        User user = await _repository.GetUserAsync(userId, HttpContext.RequestAborted);
        return UserMapper.MapUserDto(user);
    }

    [HttpPut("{userId}")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task UpdateUser(string userId, [FromBody] UpdateUserDto user)
    {
        Guard.NotNullOrEmpty(userId, nameof(userId));
        Guard.NotNull(user, nameof(user));
        Guard.NotNullOrEmpty(user.Username, nameof(user.Username));
        
        var ct = HttpContext.RequestAborted;

        var updatedUser = await _repository.UpdateUserAsync(userId,
            new User
            {
                UserId = userId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DriverLicense = user.DriverLicense,
                Verified = user.Verified,
                PasswordHash = !string.IsNullOrEmpty(user.Password)
                    ? Hasher.CalculateHash(user.Password)
                    : null,
                Roles = user.Roles
            },
            selfUpdate: false, ct);

        _eventProducer.ProduceUserUpdatedWithNoWait(updatedUser, _logger);
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task DeleteUser(string userId)
    {
        await _repository.DeleteUserAsync(userId, HttpContext.RequestAborted);
        
        _eventProducer.ProduceUserDeletedWithNoWait(userId, _logger);
    }
}