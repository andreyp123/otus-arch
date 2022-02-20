using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Dal.Repositories;

namespace RentSvc.Api.EventHandlers;

public class CarReservedEventHandler : EventHandlerBase<CarReservedMessage>
{
    private ILogger<CarReservedEventHandler> _logger;
    private readonly IRentRepository _repository;
    
    public CarReservedEventHandler(ILogger<CarReservedEventHandler> logger, IRentRepository repository)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
    }

    public override string Topic => Topics.Cars;
    public override string EventType => EventTypes.CarReserved;
    
    protected override async Task HandleMessageAsync(CarReservedMessage msg, CancellationToken ct = default)
    {
        var rent = await _repository.GetRentAsync(msg.RentId, ct);
            
        rent.StartMileage = msg.Car.Mileage;
        rent.PricePerHour = msg.Car.PricePerHour;
        rent.PricePerKm = msg.Car.PricePerKm;
        await _repository.UpdateRentAsync(msg.RentId, rent, ct);
    }
}