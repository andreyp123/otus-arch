using System;
using System.Collections.Generic;
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
using Common.Model.RentSvc;
using Common.Tracing;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Cache;
using RentSvc.Api.Extensions;
using RentSvc.Api.Helpers;
using RentSvc.Dal.Repositories;
using UserDto = Common.Model.UserSvc.UserDto;
using CarDto = Common.Model.CarSvc.CarDto;

namespace RentSvc.Api.Service;

public class RentService : IRentService
{
    private readonly ILogger<RentService> _logger;
    private readonly IRentRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IEventProducer _eventProducer;
    private readonly ICacheManager _cacheManager;
    private readonly ITracer _tracer;
    
    public RentService(ILogger<RentService> logger, IRentRepository repository, IUserRepository userRepository,
        IEventProducer eventProducer, ICacheManager cacheManager, ITracer tracer)
    {
        _logger = logger;
        _repository = repository;
        _userRepository = userRepository;
        _eventProducer = eventProducer;
        _cacheManager = cacheManager;
        _tracer = tracer;
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

    public async Task UpdateUserAsync(string userId, UserDto user, DateTime? deletedDate,
        Dictionary<string, string> tracingContext = null, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("UpdateUser", tracingContext);
        
        if (deletedDate == null)
        {
            await _userRepository.UpdateUserAsync(
                new User
                {
                    UserId = userId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    DriverLicense = user.DriverLicense,
                    Verified = user.Verified,
                    ModifiedDate = user.ModifiedDate,
                    DeletedDate = null
                }, ct);
        }
        else
        {
            await _userRepository.DeleteUserAsync(userId, deletedDate.Value, ct);
        }
    }

    public async Task<string> InitializeRentStartAsync(string userId, StartRentDto rentToStart, string idempotenceKey,
        CancellationToken ct = default)
    {
        Guard.NotNull(rentToStart, nameof(rentToStart));
        Guard.NotNullOrEmpty(rentToStart.CarId, nameof(rentToStart.CarId));
        
        using var activity = _tracer.StartActivity("InitializeRentStart");
        
        DateTime now = DateTime.UtcNow;
        
        using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // check idempotency
        if (!string.IsNullOrEmpty(idempotenceKey) &&
            !await _cacheManager.SetIfNotExistAsync($"StartRent_{idempotenceKey}", JsonHelper.Serialize(rentToStart), ct))
        {
            throw new CrashConflictException("Idempotency error");
        }
        
        // check user
        var user = await _userRepository.GetUserAsync(userId, ct);
        if (!user.Verified)
        {
            throw new CrashException($"User not verified");
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
                UserId = rent.UserId,
                TracingContext = _tracer.GetContext(activity)
            }, ct);
        
        tran.Complete();
        
        return rent.RentId;
    }

    public async Task CompleteRentStartAsync(string userId, string rentId,
        Dictionary<string, string> tracingContext = null, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("CompleteRentStart", tracingContext);
        
        using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var user = await GetUserSafeAsync(userId, ct);
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
        
        tran.Complete();
            
        // send notification
        _eventProducer.ProduceNotificationWithNoWait(
            new NotificationMessage
            {
                UserId = userId,
                UserEmail = user?.Email,
                Data = $"Rent {rentId} is started successfully",
                TracingContext = _tracer.GetContext(activity)
            },
            _logger);
    }

    public async Task FailRentStartAsync(string userId, string rentId, string errorMessage,
        Dictionary<string, string> tracingContext = null, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("FailRentStart", tracingContext);
        
        using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var user = await GetUserSafeAsync(userId, ct);
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

        tran.Complete();
            
        // send notification
        _eventProducer.ProduceNotificationWithNoWait(
            new NotificationMessage
            {
                UserId = userId,
                UserEmail = user?.Email,
                Data = $"Rent {rentId} is not started. {rent.Message}",
                TracingContext = _tracer.GetContext(activity)
            }, _logger);
    }

    public async Task UpdateInitialCarStateAsync(string userId, string rentId, CarDto car,
        Dictionary<string, string> tracingContext = null, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("UpdateInitialCarState", tracingContext);
        
        var rent = await _repository.GetUserRentAsync(userId, rentId, ct);
            
        rent.StartMileage = car.Mileage;
        rent.PricePerHour = car.PricePerHour;
        rent.PricePerKm = car.PricePerKm;
        await _repository.UpdateRentAsync(rentId, rent, ct);
    }

    public async Task UpdateRuntimeCarStateAsync(string carId, int? mileage,
        Dictionary<string, string> tracingContext = null, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("UpdateRuntimeCarState", tracingContext);
        
        await _repository.UpdateActiveRentAsync(carId, mileage, ct);
    }

    public async Task InitializeRentFinishAsync(string userId, string rentId, string idempotenceKey,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(rentId, nameof(rentId));
        
        using var activity = _tracer.StartActivity("InitializeRentFinish");

        using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        
        // check idempotency
        if (!string.IsNullOrEmpty(idempotenceKey) &&
            !await _cacheManager.SetIfNotExistAsync($"FinishRent_{idempotenceKey}", rentId, ct))
        {
            throw new CrashConflictException("Idempotency error");
        }
        
        // update rent
        var rent = await _repository.GetUserRentAsync(userId, rentId, ct);
        if (rent.State != RentState.Started)
        {
            throw new CrashException("Rent is not started");
        }
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
                UserId = rent.UserId,
                TracingContext = _tracer.GetContext(activity)
            }, ct);

        tran.Complete();
    }

    public async Task IssueInvoiceToFinishRentAsync(string userId, string rentId, CarDto car,
        Dictionary<string, string> tracingContext = null, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("IssueInvoiceToFinishRent", tracingContext);
        
        User user = null;
        Rent rent = null;
        try
        {
            user = await GetUserSafeAsync(userId, ct); 
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
                    Message = $"Time: {(endDate - rent.StartDate).ToString()}; Distance: {distance}; Amount: {amount.Value}",
                    TracingContext = _tracer.GetContext(activity)
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
                        UserEmail = user?.Email,
                        Data = $"Rent {rentId} is not finished. {rent.Message}",
                        TracingContext = _tracer.GetContext(activity)
                    }, _logger);
            }
        }
    }

    public async Task CompleteRentFinishAsync(string userId, string rentId,
        Dictionary<string, string> tracingContext = null, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("CompleteRentFinish", tracingContext);
        
        using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var user = await GetUserSafeAsync(userId, ct);
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
        
        tran.Complete();
            
        // send notification
        _eventProducer.ProduceNotificationWithNoWait(
            new NotificationMessage
            {
                UserId = rent.UserId,
                UserEmail = user?.Email,
                Data = $"Rent {rentId} is finished successfully",
                TracingContext = _tracer.GetContext(activity)
            }, _logger);
    }

    public async Task FailRentFinishAsync(string userId, string rentId, string errorMessage,
        Dictionary<string, string> tracingContext = null, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("FailRentFinish", tracingContext);
        
        using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var user = await GetUserSafeAsync(userId, ct);
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

        tran.Complete();
            
        // send notification
        _eventProducer.ProduceNotificationWithNoWait(
            new NotificationMessage
            {
                UserId = rent.UserId,
                UserEmail = user?.Email,
                Data = $"Rent {rentId} is not finished. {rent.Message}",
                TracingContext = _tracer.GetContext(activity)
            }, _logger);
    }

    private async Task<User> GetUserSafeAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            return await _userRepository.GetUserAsync(userId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get user");
            return null;
        }
    }
}