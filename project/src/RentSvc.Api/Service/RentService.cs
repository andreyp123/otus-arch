using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Common;
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Helpers;
using Common.Model;
using Common.Model.CarSvc;
using Common.Model.RentSvc;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Extensions;
using RentSvc.Api.Helpers;
using RentSvc.Dal.Repositories;

namespace RentSvc.Api.Service;

public class RentService : IRentService
{
    private readonly ILogger<RentService> _logger;
    private readonly IRentRepository _repository;
    private readonly IRequestRepository _reqRepository;
    private readonly IEventProducer _eventProducer;
    
    public RentService(ILogger<RentService> logger, IRentRepository repository, IRequestRepository reqRepository,
        IEventProducer eventProducer)
    {
        _logger = logger;
        _repository = repository;
        _reqRepository = reqRepository;
        _eventProducer = eventProducer;
    }

    public async Task<RentDto> GetUserRentAsync(string userId, string rentId, CancellationToken ct = default)
    {
        var rent = await _repository.GetUserRentAsync(userId, rentId, ct);
        return RentMapper.MapRentDto(rent, full: true);
    }

    public async Task<ListResult<RentDto>> GetUserRentsAsync(string userId, int start, int size, CancellationToken ct = default)
    {
        (Rent[] users, int total) = await _repository.GetUserRentsAsync(userId, start, size, ct);
        return new ListResult<RentDto>(
            users.Select(r => RentMapper.MapRentDto(r, full: false)).ToArray(),
            total);
    }

    public async Task<string> InitializeRentStartAsync(string userId, StartRentDto rentToStart, string idempotenceKey, CancellationToken ct = default)
    {
        Guard.NotNull(rentToStart, nameof(rentToStart));
        Guard.NotNullOrEmpty(rentToStart.CarId, nameof(rentToStart.CarId));
        
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
        await _eventProducer.ProduceEventAsync(
            new EventKey(Topics.Rents, EventTypes.RentCreated),
            new RentCreatedMessage
            {
                RentId = rent.RentId,
                CarId = rent.CarId,
                UserId = rent.UserId
            }, ct);
        
        scope.Complete();
        
        return rent.RentId;
    }

    public async Task CompleteRentStartAsync(string userId, string rentId, CancellationToken ct = default)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var rent = await _repository.GetUserRentAsync(userId, rentId, ct);
        if (rent.State != RentState.Starting)
        {
            _logger.LogError($"Rent wrong state (expected {RentState.Starting}, actual {rent.State}). Doing nothing");
            return;
        }
            
        // update rent
        rent.StartDate = DateTime.UtcNow;
        rent.State = RentState.Started;
        rent.Message = "Rent is started";
        await _repository.UpdateRentAsync(rentId, rent, ct);
        
        scope.Complete();
            
        // send notification
        _eventProducer.ProduceNotificationWithNoWait(
            new NotificationMessage
            {
                UserId = rent.UserId,
                Data = "Rent is started successfully"
            },
            _logger);
    }

    public async Task FailRentStartAsync(string userId, string rentId, string errorMessage, CancellationToken ct = default)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var rent = await _repository.GetUserRentAsync(userId, rentId, ct);
        if (rent.State != RentState.Starting)
        {
            _logger.LogError($"Rent wrong state (expected {RentState.Starting}, actual {rent.State}). Doing nothing");
            return;
        }

        // update rent
        rent.EndDate = DateTime.UtcNow;
        rent.State = RentState.Error;
        rent.Message = $"Rent starting failed. {errorMessage}";
        await _repository.UpdateRentAsync(rentId, rent, ct);

        scope.Complete();
            
        // send notification
        _eventProducer.ProduceNotificationWithNoWait(
            new NotificationMessage
            {
                UserId = rent.UserId,
                Data = $"Rent is not started. {rent.Message}"
            }, _logger);
    }

    public async Task UpdateInitialCarStateAsync(string userId, string rentId, CarDto car, CancellationToken ct = default)
    {
        var rent = await _repository.GetUserRentAsync(userId, rentId, ct);
            
        rent.StartMileage = car.Mileage;
        rent.PricePerHour = car.PricePerHour;
        rent.PricePerKm = car.PricePerKm;
        await _repository.UpdateRentAsync(rentId, rent, ct);
    }

    public async Task UpdateRuntimeCarStateAsync(string carId, int? mileage, CancellationToken ct = default)
    {
        await _repository.UpdateActiveRentAsync(carId, mileage, ct);
    }

    public async Task InitializeRentFinishAsync(string userId, string rentId, CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(rentId, nameof(rentId));
        
        var rent = await _repository.GetUserRentAsync(userId, rentId, ct);
        if (rent.State != RentState.Started)
        {
            throw new CrashException("Rent is not started");
        }
        
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        rent.EndDate = DateTime.UtcNow;
        rent.State = RentState.Finishing;
        rent.Message = "Rent is finishing";
        await _repository.UpdateRentAsync(rentId, rent, ct);
        
        // produce 'RentFinishRequested' event - initial event in the 'Start Finish' choreography saga
        await _eventProducer.ProduceEventAsync(
            new EventKey(Topics.Rents, EventTypes.RentFinishRequested),
            new RentFinishRequestedMessage
            {
                RentId = rent.RentId,
                CarId = rent.CarId,
                UserId = rent.UserId
            }, ct);

        scope.Complete();
    }

    public async Task IssueInvoiceToFinishRentAsync(string userId, string rentId, CarDto car, CancellationToken ct = default)
    {
        Rent rent = null;
        try
        {
            rent = await _repository.GetUserRentAsync(userId, rentId, ct);
            if (rent.State != RentState.Finishing)
            {
                _logger.LogError($"Rent wrong state (expected {RentState.Finishing}, actual {rent.State}). Doing nothing");
                return;
            }

            rent.Mileage = car.Mileage;
            await _repository.UpdateRentAsync(rentId, rent, ct);

            var endDate = rent.EndDate ?? DateTime.UtcNow;
            var distance = RentCalculator.CalcDistance(rent);
            if (!distance.HasValue)
            {
                throw new CrashException("Distance is not calculated");
            }
            var amount = RentCalculator.CalcAmount(rent, endDate);
            if (!distance.HasValue || !amount.HasValue)
            {
                throw new CrashException("Amount is not calculated");
            }

            await _eventProducer.ProduceEventAsync(
                new EventKey(Topics.Rents, EventTypes.RentInvoiceCreated),
                new RentInvoiceCreatedMessage
                {
                    RentId = rentId,
                    CarId = car.CarId,
                    UserId = userId,
                    Amount = amount.Value,
                    Message =
                        $"Time: {(endDate - rent.StartDate).ToString()}; Distance: {distance}; Amount: {amount.Value}"
                }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occurred", ex);
            
            if (rent != null)
            {
                rent.EndDate = null;
                rent.State = RentState.Started;
                rent.Message = $"Rent is started. Finishing attempt failed. {ex.Message}";
                await _repository.UpdateRentAsync(rentId, rent, ct);

                // send notification
                _eventProducer.ProduceNotificationWithNoWait(
                    new NotificationMessage
                    {
                        UserId = userId,
                        Data = $"Rent is not finished. {rent.Message}"
                    }, _logger);
            }
        }
    }

    public async Task CompleteRentFinishAsync(string userId, string rentId, CancellationToken ct = default)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var rent = await _repository.GetUserRentAsync(userId, rentId, ct);
        if (rent.State != RentState.Finishing)
        {
            _logger.LogError($"Rent wrong state (expected {RentState.Finishing}, actual {rent.State}). Doing nothing");
            return;
        }
            
        // update rent
        rent.EndDate ??= DateTime.UtcNow;
        rent.State = RentState.Finished;
        rent.Message = "Rent is finished";
        await _repository.UpdateRentAsync(rentId, rent, ct);
        
        scope.Complete();
            
        // send notification
        _eventProducer.ProduceNotificationWithNoWait(
            new NotificationMessage
            {
                UserId = rent.UserId,
                Data = "Rent is finished successfully"
            }, _logger);
    }

    public async Task FailRentFinishAsync(string userId, string rentId, string errorMessage, CancellationToken ct = default)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var rent = await _repository.GetUserRentAsync(userId, rentId, ct);
        if (rent.State != RentState.Finishing)
        {
            _logger.LogError($"Rent wrong state (expected {RentState.Finishing}, actual {rent.State}). Doing nothing");
            return;
        }

        // update rent
        rent.EndDate = null;
        rent.State = RentState.Started;
        rent.Message = $"Rent is started. Finishing attempt failed. {errorMessage}";
        await _repository.UpdateRentAsync(rentId, rent, ct);

        scope.Complete();
            
        // send notification
        _eventProducer.ProduceNotificationWithNoWait(
            new NotificationMessage
            {
                UserId = rent.UserId,
                Data = $"Rent is not finished. {rent.Message}"
            }, _logger);
    }
}