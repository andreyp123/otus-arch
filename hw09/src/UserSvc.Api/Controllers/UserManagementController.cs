using Common;
using Common.Helpers;
using Common.Model;
using Common.Model.UserSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using UserSvc.Dal.Repositories;

namespace UserSvc.Api.Controllers;

[ApiController]
[Route("users")]
public class UserManagementController : ControllerBase
{
    private readonly ILogger<UserManagementController> _logger;
    private readonly IUserRepository _repository;

    public UserManagementController(ILogger<UserManagementController> logger, IUserRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [HttpPost("")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task<string> CreateUser([FromBody] UpdateUserDto user)
    {
        Guard.NotNull(user, nameof(user));
        Guard.NotNullOrEmpty(user.Username, nameof(user.Username));
        Guard.NotNullOrEmpty(user.Password, nameof(user.Password));

        return await _repository.CreateUserAsync(
            new User
            {
                UserId = IdGenerator.Generate(),
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DriverLicense = user.DriverLicense,
                Verified = user.Verified,
                PasswordHash = Hasher.CalculateHash(user.Password),
                Roles = user.Roles
            },
            HttpContext.RequestAborted);
    }

    [HttpGet("")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task<ListResult<UserDto>> GetUsers([FromQuery] int start, [FromQuery] int size)
    {
        (User[] users, int total) = await _repository.GetUsersAsync(start, size, HttpContext.RequestAborted);
        return new ListResult<UserDto>(
            users.Select(u => MapUserDto(u)).ToArray(),
            total);
    }

    [HttpGet("{userId}")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task<UserDto> GetUser(string userId)
    {
        User user = await _repository.GetUserAsync(userId, HttpContext.RequestAborted);
        return MapUserDto(user);
    }

    [HttpPut("{userId}")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task UpdateUser(string userId, [FromBody] UpdateUserDto user)
    {
        Guard.NotNullOrEmpty(userId, nameof(userId));
        Guard.NotNull(user, nameof(user));
        Guard.NotNullOrEmpty(user.Username, nameof(user.Username));

        await _repository.UpdateUserAsync(userId,
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
            selfUpdate: false,
            HttpContext.RequestAborted);
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task DeleteUser(string userId)
    {
        await _repository.DeleteUserAsync(userId, HttpContext.RequestAborted);
    }

    private UserDto MapUserDto(User user)
    {
        return new UserDto
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
}