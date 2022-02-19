using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Model.RentSvc;
using Microsoft.Extensions.Logging;
using RentSvc.Dal.Repositories;

namespace RentSvc.Api.EventHandlers;

public class CarEventHandler : EventHandlerBase
{
    private readonly ILogger<CarEventHandler> _logger;
    private readonly IRentRepository _repository;
    private readonly IEventProducer _eventProducer;

    public CarEventHandler(ILogger<CarEventHandler> logger, IRentRepository repository, IEventProducer eventProducer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
        
        RegisterEventType(EventType.CarStateUpdated, HandleCarStateAsync);
        RegisterEventType(EventType.CarReserved, HandleCarReservedAsync);
        RegisterEventType(EventType.CarReservationFailed, HandleCarReservationFailedAsync);
    }
    
    public override string Topic => Topics.Cars;

    private async Task HandleCarStateAsync(ConsumedEvent ev, CancellationToken ct)
    {
        await HandleWithPayload(ev, async (CarStateMessage message) =>
        {
            await _repository.UpdateActiveRentAsync(message.CarId, message.Mileage, ct);
        });
    }

    private async Task HandleCarReservedAsync(ConsumedEvent ev, CancellationToken ct)
    {
        await HandleWithPayload(ev, async (CarReservedMessage message) =>
        {
            var rent = await _repository.GetRentAsync(message.RentId, ct);
            
            rent.StartMileage = message.Car.Mileage;
            rent.PricePerHour = message.Car.PricePerHour;
            rent.PricePerKm = message.Car.PricePerKm;
            await _repository.UpdateRentAsync(message.RentId, rent, ct);
        });
    }

    private async Task HandleCarReservationFailedAsync(ConsumedEvent ev, CancellationToken ct)
    {
        await HandleWithPayload(ev, async (CarReservationFailedMessage message) =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var rent = await _repository.GetRentAsync(message.RentId, ct);
            if (rent.State != RentState.Starting)
            {
                _logger.LogError($"Rent wrong state (expected {RentState.Starting}, actual {rent.State}). Doing nothing");
                return;
            }

            // update rent
            rent.EndDate = DateTime.UtcNow;
            rent.State = RentState.Error;
            rent.Message = $"Rent starting failed. {message.Message}";
            await _repository.UpdateRentAsync(message.RentId, rent, ct);
            
            // send notification
            await _eventProducer.ProduceEventAsync(Topics.Notifications, new ProducedEvent<NotificationMessage>
            {
                Type = EventType.Notification,
                Payload = new NotificationMessage
                {
                    UserId = rent.UserId,
                    Data = $"Rent is not started. {rent.Message}"
                }
            }, ct);

            scope.Complete();
        });
    }
}