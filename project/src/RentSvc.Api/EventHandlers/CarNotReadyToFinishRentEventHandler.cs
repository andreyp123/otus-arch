using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class CarNotReadyToFinishRentEventHandler : EventHandlerBase<CarNotReadyToFinishRentMessage>
{
    private readonly ILogger<CarNotReadyToFinishRentEventHandler> _logger;
    private readonly IRentService _rentService;
    
    public CarNotReadyToFinishRentEventHandler(ILogger<CarNotReadyToFinishRentEventHandler> logger, IRentService rentService)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentService;
    }

    public override string Topic => Topics.Cars;
    public override string EventType => EventTypes.CarNotReadyToFinishRent;
    
    protected override async Task HandleMessageAsync(CarNotReadyToFinishRentMessage msg, CancellationToken ct = default)
    {
        await _rentService.FailRentFinishAsync(msg.UserId, msg.RentId, msg.Message, ct);
    }
}