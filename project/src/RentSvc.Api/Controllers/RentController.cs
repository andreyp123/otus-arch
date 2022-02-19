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
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using RentSvc.Dal.Repositories;

namespace RentSvc.Api.Controllers;

[ApiController]
[Route("rents")]
public class RentController : ControllerBase
{
    private readonly ILogger<RentController> _logger;
    private readonly IRentRepository _repository;
    private readonly IRequestRepository _reqRepository;
    private readonly IEventProducer _eventProducer;

    public RentController(ILogger<RentController> logger, IRentRepository repository,
        IRequestRepository reqRepository, IEventProducer eventProducer)
    {
        _logger = logger;
        _repository = repository;
        _reqRepository = reqRepository;
        _eventProducer = eventProducer;
    }

    [HttpPost]
    [Authorize]
    public async Task<string> StartRent(
        [FromHeader(Name = "Idempotence-Key")] string idempotenceKey,
        [FromBody] StartRentDto rentToStart)
    {
        Guard.NotNull(rentToStart, nameof(rentToStart));
        Guard.NotNullOrEmpty(rentToStart.CarId, nameof(rentToStart.CarId));
        
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
        
        // check user for active rents
        if (await _repository.HasUserActiveRentsAsync(userId, ct))
        {
            throw new CrashException("User has active rents");
        }

        // create rent
        var rent = new Rent
        {
            RentId = Generator.GenerateId(),
            UserId = userId,
            CarId = rentToStart.CarId,
            Data = rentToStart.Data,
            State = RentState.Starting,
            CreatedDate = now,
            Message = "Rent is starting"
        };
        await _repository.CreateRentAsync(rent, ct);

        // produce 'RentCreated' event - initial event in the 'Start Rent' choreography saga
        await _eventProducer.ProduceEventAsync(Topics.Rents,
            new ProducedEvent<RentCreatedMessage>
            {
                Type = EventType.RentCreated,
                Payload = new RentCreatedMessage
                {
                    RentId = rent.RentId,
                    CarId = rent.CarId,
                    UserId = rent.UserId
                }
            }, ct);
        
        scope.Complete();
        
        return rent.RentId;
    }

    [HttpGet("{rentId}")]
    [Authorize]
    public async Task<RentDto> GetRent(string rentId)
    {
        var rent = await _repository.GetRentAsync(rentId, HttpContext.RequestAborted);
        return MapRentDto(rent, true);
    }

    [HttpGet]
    [Authorize]
    public async Task<ListResult<RentDto>> GetUserRents([FromQuery] int start, [FromQuery] int size)
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        (Rent[] users, int total) = await _repository.GetUserRentsAsync(userId, start, size, HttpContext.RequestAborted);
        return new ListResult<RentDto>(
            users.Select(r => MapRentDto(r, false)).ToArray(),
            total);
    }

    [HttpPost("{rentId}/finish")]
    [Authorize]
    public async Task FinishRent(string rentId)
    {
        var rent = await _repository.GetRentAsync(rentId, HttpContext.RequestAborted);
        if (rent.State != RentState.Started)
        {
            throw new CrashException("Rent is not started");
        }
        
        // todo: SAGA ('Finish Rent' choreography saga)

        rent.EndDate = DateTime.UtcNow;
        rent.State = RentState.Finished;
        rent.Message = "Rent is finished";
    
        await _repository.UpdateRentAsync(rentId, rent, HttpContext.RequestAborted);
    }

    private RentDto MapRentDto(Rent rent, bool full = false)
    {
        var rentDto = new RentDto
        {
            RentId = rent.RentId,
            UserId = rent.UserId,
            CarId = rent.CarId,
            Data = rent.Data,
            CreatedDate = rent.CreatedDate,
            StartDate = rent.StartDate,
            EndDate = rent.EndDate,
            State = rent.State.ToString(),
            Message = rent.Message
        };

        if (full)
        {
            DateTime now = DateTime.UtcNow;
            rentDto.Distance = CalcDistance(rent);
            rentDto.Amount = CalcAmount(rent, now);
        }

        return rentDto;
    }

    private decimal? CalcDistance(Rent rent)
    {
        return rent.StartMileage != null && rent.Mileage != null
            ? rent.Mileage - rent.StartMileage
            : null;
    }

    private decimal? CalcAmount(Rent rent, DateTime now)
    {
        if (rent.PricePerKm == null || rent.PricePerHour == null)
            return null;
        
        var distance = CalcDistance(rent);
        if (!distance.HasValue)
            return null;
        
        if (!rent.StartDate.HasValue)
            return null;

        var hours = (decimal)((rent.EndDate ?? now) - rent.StartDate.Value).TotalHours;

        return rent.PricePerHour * hours + rent.PricePerKm * distance;
    }
}