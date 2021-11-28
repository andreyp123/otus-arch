using UserManager.WebApi.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace UserManager.WebApi.Controllers
{
    [ApiController]
    public class BasicController : ControllerBase
    {
        private readonly ILogger<BasicController> _logger;
        private readonly VersionInfo _versionInfo;

        public BasicController(ILogger<BasicController> logger)
        {
            _logger = logger;
            _versionInfo = new VersionInfo
            {
                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                Hostname = Dns.GetHostName()
            };
        }

        [HttpGet("")]
        [AllowAnonymous]
        public string Hello()
        {
            return $"Hello from UserManagerTest!";
        }

        [HttpGet("version")]
        [AllowAnonymous]
        public VersionInfo GetVersion()
        {
            return _versionInfo;
        }
    }
}
