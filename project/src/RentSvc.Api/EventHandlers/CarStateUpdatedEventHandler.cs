using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class CarStateUpdatedEventHandler : EventHandlerBase<CarStateUpdatedMessage>
{
    private readonly ILogger<CarStateUpdatedEventHandler> _logger;
    private readonly IRentService _rentService;
    
    public CarStateUpdatedEventHandler(ILogger<CarStateUpdatedEventHandler> logger, IRentService rentService)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentService;
    }

    public override string Topic => Topics.Cars;
    public override string EventType => EventTypes.CarStateUpdated;
    
    protected override async Task HandleMessageAsync(CarStateUpdatedMessage msg, CancellationToken ct = default)
    {
        await _rentService.UpdateRuntimeCarStateAsync(msg.CarId, msg.Mileage, ct);
    }
}