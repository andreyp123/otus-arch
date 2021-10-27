using UserManager.API.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;

namespace UserManager.API.Controllers
{
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly VersionInfo _versionInfo;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
            _versionInfo = new VersionInfo
            {
                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                Hostname = Dns.GetHostName()
            };
        }

        [HttpGet]
        [Route("")]
        public string Hello()
        {
            return $"Hello from UserManagerTest!";
        }

        [HttpGet]
        [Route("health")]
        public HealthResult CheckHealth()
        {
            return HealthResult.OK;
        }

        [HttpGet]
        [Route("version")]
        public VersionInfo GetVersion()
        {
            return _versionInfo;
        }
    }
}
