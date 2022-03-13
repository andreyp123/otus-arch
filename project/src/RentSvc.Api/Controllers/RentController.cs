using Common.Model.RentSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Model;
using RentSvc.Api.Service;

namespace RentSvc.Api.Controllers;

[ApiController]
[Route("rents")]
public class RentController : ControllerBase
{
    private readonly ILogger<RentController> _logger;
    private readonly IRentService _rentService;

    public RentController(ILogger<RentController> logger, IRentService rentService)
    {
        _logger = logger;
        _rentService = rentService;
    }

    [HttpPost]
    [Authorize]
    public async Task<string> StartRent(
        [FromHeader(Name = "Idempotence-Key")] string idempotenceKey,
        [FromBody] StartRentDto rentToStart)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return await _rentService.InitializeRentStartAsync(userId, rentToStart, idempotenceKey,
            HttpContext.RequestAborted);
    }

    [HttpGet("{rentId}")]
    [Authorize]
    public async Task<RentDto> GetUserRent(string rentId)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return await _rentService.GetUserRentAsync(userId, rentId, HttpContext.RequestAborted);
    }

    [HttpGet]
    [Authorize]
    public async Task<ListResult<RentDto>> GetUserRents([FromQuery] int start, [FromQuery] int size)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return await _rentService.GetUserRentsAsync(userId, start, size, HttpContext.RequestAborted);
    }

    [HttpPost("{rentId}/finish")]
    [Authorize]
    public async Task FinishRent(
        [FromHeader(Name = "Idempotence-Key")] string idempotenceKey,
        string rentId)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        await _rentService.InitializeRentFinishAsync(userId, rentId, idempotenceKey,
            HttpContext.RequestAborted);
    }
}