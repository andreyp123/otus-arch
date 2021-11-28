using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManager.BusinessLogic;
using System.Threading.Tasks;
using UserManager.Common.Model;
using Microsoft.AspNetCore.Authorization;

namespace UserManager.WebApi.Controllers
{
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly ILogger<UserAuthController> _logger;
        private readonly IBusinessLogic _businessLogic;

        public UserAuthController(ILogger<UserAuthController> logger, IBusinessLogic businessLogic)
        {
            _logger = logger;
            _businessLogic = businessLogic;
        }

        [HttpGet("auth/token")]
        [AllowAnonymous]
        public async Task<UserSecurityToken> Login(string username, string password)
        {
            return await _businessLogic.AuthUserAsync(username, password, HttpContext.RequestAborted);
        }
    }
}
