using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Dal.Repositories;

namespace RentSvc.Api.EventHandlers;

public class CarStateUpdatedEventHandler : EventHandlerBase<CarStateUpdatedMessage>
{
    private ILogger<CarStateUpdatedEventHandler> _logger;
    private readonly IRentRepository _repository;
    
    public CarStateUpdatedEventHandler(ILogger<CarStateUpdatedEventHandler> logger, IRentRepository repository)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
    }

    public override string Topic => Topics.Cars;
    public override string EventType => EventTypes.CarStateUpdated;
    
    protected override async Task HandleMessageAsync(CarStateUpdatedMessage msg, CancellationToken ct = default)
    {
        await _repository.UpdateActiveRentAsync(msg.CarId, msg.Mileage, ct);
        
        _logger.LogInformation("Updated active rent in the DB");
    }
}