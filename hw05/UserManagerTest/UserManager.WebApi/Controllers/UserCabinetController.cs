using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManager.BusinessLogic;
using UserManager.Common.Model.API;

namespace UserManager.WebApi.Controllers
{
    [ApiController]
    public class UserCabinetController : ControllerBase
    {
        private readonly ILogger<UserCabinetController> _logger;
        private readonly IBusinessLogic _businessLogic;

        public UserCabinetController(ILogger<UserCabinetController> logger, IBusinessLogic businessLogic)
        {
            _logger = logger;
            _businessLogic = businessLogic;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task RegisterCurrentUser([FromBody] UserDto user)
        {
            await _businessLogic.CreateUserAsync(user, selfCreate: true, HttpContext.RequestAborted);
        }

        [HttpGet("cabinet")]
        [Authorize]
        public async Task<UserDto> GetCurrentUser()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return await _businessLogic.GetUserAsync(userId, HttpContext.RequestAborted);
        }

        [HttpPut("cabinet")]
        [Authorize]
        public async Task UpdateCurrentUser([FromBody] UserDto user)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            await _businessLogic.UpdateUserAsync(userId, user, selfUpdate: true, HttpContext.RequestAborted);
        }
    }
}
