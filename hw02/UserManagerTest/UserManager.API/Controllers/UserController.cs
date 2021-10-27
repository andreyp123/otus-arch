using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UserManager.Common;

namespace UserManager.API.Controllers
{

    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("")]
        public async Task<string> CreateUser([FromBody] User user)
        {
            return Guid.NewGuid().ToString();
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<User> GetUser(string userId)
        {
            return new User();
        }

        [HttpPut]
        [Route("{userId}")]
        public async Task UpdateUser(string userId, [FromBody] User user)
        {
            //
        }

        [HttpDelete]
        [Route("{userId}")]
        public async Task DeleteUser(string userId)
        {
            //
        }
    }
}
