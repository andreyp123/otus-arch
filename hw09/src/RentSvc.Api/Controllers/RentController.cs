using Common;
using Common.Helpers;
using Common.Model.RentSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Model;
using System;
using System.Threading;
using System.Transactions;
using Common.Model.NotificationSvc;
using RentSvc.Api.KafkaProducer;
using RentSvc.Dal.Repositories;

namespace RentSvc.Api.Controllers;

[ApiController]
[Route("rents")]
public class RentController : ControllerBase
{
    private const string NOTIFICATIONS_TOPIC = "notifications";
    
    private readonly ILogger<RentController> _logger;
    private readonly IRentRepository _repository;
    private readonly IRequestRepository _reqRepository;
    private readonly IKafkaProducer _kafkaProducer;

    public RentController(ILogger<RentController> logger, IRentRepository repository,
        IRequestRepository reqRepository, IKafkaProducer kafkaProducer)
    {
        _logger = logger;
        _repository = repository;
        _reqRepository = reqRepository;
        _kafkaProducer = kafkaProducer;
    }

    [HttpPost]
    [Authorize]
    public async Task<string> StartRent(
        [FromHeader(Name = "Idempotence-Key")] string idempotenceKey,
        [FromBody] StartRentDto startRent)
    {
        Guard.NotNull(startRent, nameof(startRent));
        Guard.NotNullOrEmpty(startRent.CarId, nameof(startRent.CarId));
        
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        CancellationToken ct = HttpContext.RequestAborted;
        DateTime now = DateTime.UtcNow;
        
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // check request for idempotency
        if (!string.IsNullOrEmpty(idempotenceKey) &&
            !await _reqRepository.CheckCreateRequestAsync(idempotenceKey, "StartRent", now, ct))
        {
            throw new CrashConflictException("Idempotency error");
        }

        // create rent
        var rent = new Rent
        {
            RentId = IdGenerator.Generate(),
            UserId = userId,
            CarId = startRent.CarId,
            Data = startRent.Data,
            State = RentState.Starting,
            CreatedDate = now,
            Message = "",
            Distance = 0,
            Amount = 0
        };
        await _repository.CreateRentAsync(rent, ct);
        
        scope.Complete();
        
        // update billing
        // todo: SAGA
        
        rent.StartDate = DateTime.UtcNow;
        rent.State = RentState.Started;
        rent.Message = "Rent is started";
    
        await _repository.UpdateRentAsync(rent.RentId, rent, HttpContext.RequestAborted);
        
        // send notification
        Task.Run(() => _kafkaProducer.SendAsync(NOTIFICATIONS_TOPIC, PrepareMessage(rent), ct));

        return rent.RentId;
    }

    [HttpGet("{rentId}")]
    [Authorize]
    public async Task<RentDto> GetRent(string rentId)
    {
        var rent = await _repository.GetRentAsync(rentId, HttpContext.RequestAborted);
        return MapRentDto(rent);
    }

    [HttpGet]
    [Authorize]
    public async Task<ListResult<RentDto>> GetUserRents([FromQuery] int start, [FromQuery] int size)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        (Rent[] users, int total) = await _repository.GetUserRentsAsync(userId, start, size, HttpContext.RequestAborted);
        return new ListResult<RentDto>(
            users.Select(MapRentDto).ToArray(),
            total);
    }

    [HttpPost("{rentId}/finish")]
    [Authorize]
    public async Task FinishRent(string rentId)
    {
        var rent = await _repository.GetRentAsync(rentId, HttpContext.RequestAborted);
        
        // todo: check rent state
        
        // todo: SAGA

        rent.EndDate = DateTime.UtcNow;
        rent.State = RentState.Finished;
        rent.Message = "Rent is finished";
    
        await _repository.UpdateRentAsync(rentId, rent, HttpContext.RequestAborted);
    }

    private RentDto MapRentDto(Rent rent)
    {
        return new RentDto
        {
            RentId = rent.RentId,
            UserId = rent.UserId,
            CarId = rent.CarId,
            Data = rent.Data,
            CreatedDate = rent.CreatedDate,
            StartDate = rent.StartDate,
            EndDate = rent.EndDate,
            State = rent.State.ToString(),
            Message = rent.Message,
            Distance = rent.Distance,
            Amount = rent.Amount
        };
    }

    private NotificationMessage PrepareMessage(Rent rent)
    {
        return new NotificationMessage
        {
            UserId = rent.UserId,
            Data = $"Rent: {rent.RentId}"
        };
    }
}