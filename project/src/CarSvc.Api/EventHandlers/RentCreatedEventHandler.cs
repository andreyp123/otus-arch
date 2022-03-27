using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using CarSvc.Api.Helpers;
using CarSvc.Dal.Repositories;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Tracing;
using Microsoft.Extensions.Logging;

namespace CarSvc.Api.EventHandlers;

public class RentCreatedEventHandler : EventHandlerBase<RentCreatedMessage>
{
    private readonly ILogger<RentCreatedEventHandler> _logger;
    private readonly ICarRepository _repository;
    private readonly IEventProducer _eventProducer;
    private readonly ITracer _tracer;

    public RentCreatedEventHandler(ILogger<RentCreatedEventHandler> logger, ICarRepository repository,
        IEventProducer eventProducer, ITracer tracer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
        _tracer = tracer;
    }
    
    public override string Topic => Topics.Rents;
    public override string EventType => EventTypes.RentCreated;

    protected override async Task HandleMessageAsync(RentCreatedMessage msg, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("HandleRentCreated", msg.TracingContext);
        
        try
        {
            using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var car = await _repository.StartCarRent(msg.CarId, msg.RentId, DateTime.UtcNow, ct);

            await _eventProducer.ProduceEventAsync(
                new EventKey(Topics.Cars, EventTypes.CarReserved),
                new CarReservedMessage
                {
                    RentId = msg.RentId,
                    UserId = msg.UserId,
                    Car = CarMapper.MapCarDto(car),
                    TracingContext = _tracer.GetContext(activity)
                }, ct);

            tran.Complete();
        }
        catch (Exception ex)
        {
            await _eventProducer.ProduceEventAsync(
                new EventKey(Topics.Cars, EventTypes.CarReservationFailed),
                new CarReservationFailedMessage
                {
                    RentId = msg.RentId,
                    CarId = msg.CarId,
                    UserId = msg.UserId,
                    Message = $"Car reservation failed. {ex.Message}",
                    TracingContext = _tracer.GetContext(activity)
                }, ct);
        }
    }
}