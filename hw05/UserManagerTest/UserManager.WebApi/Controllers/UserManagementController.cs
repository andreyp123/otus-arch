using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UserManager.BusinessLogic;
using UserManager.Common.Model;
using UserManager.Common.Model.API;

namespace UserManager.WebApi.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserManagementController : ControllerBase
    {
        private readonly ILogger<UserManagementController> _logger;
        private readonly IBusinessLogic _businessLogic;

        public UserManagementController(ILogger<UserManagementController> logger, IBusinessLogic businessLogic)
        {
            _logger = logger;
            _businessLogic = businessLogic;
        }

        [HttpPost("")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<string> CreateUser([FromBody] UserDto user)
        {
            return await _businessLogic.CreateUserAsync(user, selfCreate: false, HttpContext.RequestAborted);
        }

        [HttpGet("")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ListResult<UserDto>> GetUsers([FromQuery] int start, [FromQuery] int size)
        {
            return await _businessLogic.GetUsersAsync(start, size, HttpContext.RequestAborted);
        }

        [HttpGet("{userId}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<UserDto> GetUser(string userId)
        {
            return await _businessLogic.GetUserAsync(userId, HttpContext.RequestAborted);
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task UpdateUser(string userId, [FromBody] UserDto user)
        {
            await _businessLogic.UpdateUserAsync(userId, user, selfUpdate: false, HttpContext.RequestAborted);
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task DeleteUser(string userId)
        {
            await _businessLogic.DeleteUserAsync(userId, HttpContext.RequestAborted);
        }
    }
}
