using Common.Helpers;
using Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RentSvc.Api.Controllers;

[ApiController]
public class BasicController : ControllerBase
{
    private readonly ILogger<BasicController> _logger;
    private readonly VersionInfo _versionInfo;

    public BasicController(ILogger<BasicController> logger)
    {
        _logger = logger;
        _versionInfo = VersionHelper.GetVersionInfo();
    }

    [HttpGet("")]
    [AllowAnonymous]
    public string Hello()
    {
        return $"Hello from Rent Service!";
    }

    [HttpGet("version")]
    [AllowAnonymous]
    public VersionInfo GetVersion()
    {
        return _versionInfo;
    }
}
