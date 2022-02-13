using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using CarSvc.Api.Helpers;
using CarSvc.Dal.Repositories;
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using Microsoft.Extensions.Logging;

namespace CarSvc.Api.EventHandlers;

public class RentEventHandler : EventHandlerBase
{
    private readonly ILogger<RentEventHandler> _logger;
    private readonly ICarRepository _repository;
    private readonly IEventProducer _eventProducer;

    public RentEventHandler(ILogger<RentEventHandler> logger, ICarRepository repository, IEventProducer eventProducer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
        
        RegisterEventType(EventType.RentCreated, HandleRentCreatedAsync);
    }
    
    public override string Topic => Topics.Rents;

    private async Task HandleRentCreatedAsync(ConsumedEvent ev, CancellationToken ct)
    {
        await HandleWithPayload(ev, async (RentCreatedMessage message) =>
        {
            try
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                
                var car = await _repository.StartCarRent(message.CarId, message.RentId, DateTime.UtcNow, ct);
                
                await _eventProducer.ProduceEventAsync(Topics.Cars, new ProducedEvent<CarReservedMessage>
                {
                    Type = EventType.CarReserved,
                    Payload = new CarReservedMessage
                    {
                        RentId = message.RentId,
                        UserId = message.UserId,
                        Car = CarMapper.MapCarDto(car)
                    }
                }, ct);
                
                scope.Complete();
            }
            catch (Exception ex)
            {
                await _eventProducer.ProduceEventAsync(Topics.Cars, new ProducedEvent<CarReservationFailedMessage>
                {
                    Type = EventType.CarReservationFailed,
                    Payload = new CarReservationFailedMessage
                    {
                        RentId = message.RentId,
                        CarId = message.CarId,
                        UserId = message.UserId,
                        Message = $"Car reservation failed. {ex.Message}"
                    }
                }, ct);
            }
        });
    }
}