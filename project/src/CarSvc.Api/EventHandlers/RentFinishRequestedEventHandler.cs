using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using CarSvc.Api.Helpers;
using CarSvc.Dal.Repositories;
using Common;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Model.CarSvc;
using Common.Tracing;
using Microsoft.Extensions.Logging;

namespace CarSvc.Api.EventHandlers;

public class RentFinishRequestedEventHandler : EventHandlerBase<RentFinishRequestedMessage>
{
    private readonly ILogger<RentFinishRequestedEventHandler> _logger;
    private readonly ICarRepository _repository;
    private readonly IEventProducer _eventProducer;
    private readonly ITracer _tracer;
    
    public RentFinishRequestedEventHandler(ILogger<RentFinishRequestedEventHandler> logger, ICarRepository repository,
        IEventProducer eventProducer, ITracer tracer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
        _tracer = tracer;
    }

    public override string Topic => Topics.Rents;
    public override string EventType => EventTypes.RentFinishRequested;
    
    protected override async Task HandleMessageAsync(RentFinishRequestedMessage msg, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("HandleRentFinishRequested", msg.TracingContext);
        
        try
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var car = await _repository.GetCarAsync(msg.CarId, ct);
            if (car.DriveState != CarDriveState.Off)
            {
                throw new CrashException("Drive state is not OFF");
            }
            if (car.RemainingFuel < 10)
            {
                throw new CrashException("Low fuel");
            }

            await _eventProducer.ProduceEventAsync(
                new EventKey(Topics.Cars, EventTypes.CarReadyToFinishRent),
                new CarReadyToFinishRentMessage
                {
                    RentId = msg.RentId,
                    UserId = msg.UserId,
                    Car = CarMapper.MapCarDto(car),
                    TracingContext = _tracer.GetContext(activity)
                }, ct);

            scope.Complete();
        }
        catch (Exception ex)
        {
            await _eventProducer.ProduceEventAsync(
                new EventKey(Topics.Cars, EventTypes.CarNotReadyToFinishRent),
                new CarNotReadyToFinishRentMessage
                {
                    RentId = msg.RentId,
                    CarId = msg.CarId,
                    UserId = msg.UserId,
                    Message = $"Car not ready to finish rent. {ex.Message}",
                    TracingContext = _tracer.GetContext(activity)
                }, ct);
        }
    }
}