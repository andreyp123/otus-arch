using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class CarReservedEventHandler : EventHandlerBase<CarReservedMessage>
{
    private ILogger<CarReservedEventHandler> _logger;
    private readonly IRentService _rentService;
    
    public CarReservedEventHandler(ILogger<CarReservedEventHandler> logger, IRentService rentService)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentService;
    }

    public override string Topic => Topics.Cars;
    public override string EventType => EventTypes.CarReserved;
    
    protected override async Task HandleMessageAsync(CarReservedMessage msg, CancellationToken ct = default)
    {
        await _rentService.UpdateInitialCarStateAsync(msg.UserId, msg.RentId, msg.Car, msg.TracingContext, ct);
    }
}