using Common;
using Common.Helpers;
using Common.Model.UserSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UserSvc.Repository;

namespace UserSvc.Api.Controllers
{
    [ApiController]
    public class UserCabinetController : ControllerBase
    {
        private readonly ILogger<UserCabinetController> _logger;
        private readonly IUserRepository _repository;

        public UserCabinetController(ILogger<UserCabinetController> logger, IUserRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task RegisterCurrentUser([FromBody] UserDto user)
        {
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(user.Username, nameof(user.Username));
            Guard.NotNullOrEmpty(user.Password, nameof(user.Password));

            await _repository.CreateUserAsync(
                new User
                {
                    UserId = IdGenerator.Generate(),
                    Username = user.Username,
                    Email = user.Email,
                    PasswordHash = Hasher.CalculateHash(user.Password),
                    Roles = new[] { UserRoles.User }
                },
                HttpContext.RequestAborted);
        }

        [HttpGet("cabinet")]
        [Authorize]
        public async Task<UserDto> GetCurrentUser()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            User user = await _repository.GetUserAsync(userId, HttpContext.RequestAborted);
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Password = null,
                Roles = user.Roles
            };
        }

        [HttpPut("cabinet")]
        [Authorize]
        public async Task UpdateCurrentUser([FromBody] UserDto user)
        {
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(user.Username, nameof(user.Username));

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            await _repository.UpdateUserAsync(userId,
                new User
                {
                    UserId = userId,
                    Username = user.Username,
                    Email = user.Email,
                    PasswordHash = !string.IsNullOrEmpty(user.Password)
                        ? Hasher.CalculateHash(user.Password)
                        : null,
                    Roles = user.Roles
                },
                false,
                HttpContext.RequestAborted);
        }
    }
}
