using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Common.Tracing;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class CarStateUpdatedEventHandler : EventHandlerBase<CarStateUpdatedMessage>
{
    private readonly ILogger<CarStateUpdatedEventHandler> _logger;
    private readonly IRentService _rentService;
    private readonly ITracer _tracer;
    
    public CarStateUpdatedEventHandler(ILogger<CarStateUpdatedEventHandler> logger, IRentService rentService, ITracer tracer)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentService;
        _tracer = tracer;
    }

    public override string Topic => Topics.Cars;
    public override string EventType => EventTypes.CarStateUpdated;
    
    protected override async Task HandleMessageAsync(CarStateUpdatedMessage msg, CancellationToken ct = default)
    {
        await _rentService.UpdateRuntimeCarStateAsync(msg.CarId, msg.Mileage, msg.TracingContext, ct);
    }
}