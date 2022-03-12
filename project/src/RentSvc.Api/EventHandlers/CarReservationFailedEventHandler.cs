using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class CarReservationFailedEventHandler : EventHandlerBase<CarReservationFailedMessage>
{
    private ILogger<CarReservationFailedEventHandler> _logger;
    private readonly IRentService _rentService;
    
    public CarReservationFailedEventHandler(ILogger<CarReservationFailedEventHandler> logger, IRentService rentService)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentService;
    }

    public override string Topic => Topics.Cars;
    public override string EventType => EventTypes.CarReservationFailed;

    protected override async Task HandleMessageAsync(CarReservationFailedMessage msg, CancellationToken ct = default)
    {
        await _rentService.FailRentStartAsync(msg.UserId, msg.RentId, msg.Message, ct);
    }
}