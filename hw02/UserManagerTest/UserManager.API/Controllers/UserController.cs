﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UserManager.Common;
using UserManager.DAL;

namespace UserManager.API.Controllers
{

    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserRepository _repository;

        public UserController(ILogger<UserController> logger, IUserRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpPost("")]
        public async Task<string> CreateUser([FromBody] User user)
        {
            return await _repository.CreateUserAsync(user, HttpContext.RequestAborted);
        }

        [HttpGet("{userId}")]
        public async Task<User> GetUser(string userId)
        {
            return await _repository.GetUserAsync(userId, HttpContext.RequestAborted);
        }

        [HttpPut("{userId}")]
        public async Task UpdateUser(string userId, [FromBody] User user)
        {
            await _repository.UpdateUserAsync(userId, user, HttpContext.RequestAborted);
        }

        [HttpDelete("{userId}")]
        public async Task DeleteUser(string userId)
        {
            await _repository.DeleteUserAsync(userId, HttpContext.RequestAborted);
        }
    }
}
