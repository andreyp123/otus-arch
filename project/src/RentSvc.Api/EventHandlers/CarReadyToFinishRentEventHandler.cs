using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class CarReadyToFinishRentEventHandler : EventHandlerBase<CarReadyToFinishRentMessage>
{
    private readonly ILogger<CarReadyToFinishRentEventHandler> _logger;
    private readonly IRentService _rentService;
    
    public CarReadyToFinishRentEventHandler(ILogger<CarReadyToFinishRentEventHandler> logger, IRentService rentService)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentService;
    }

    public override string Topic => Topics.Cars;
    public override string EventType => EventTypes.CarReadyToFinishRent;
    
    protected override async Task HandleMessageAsync(CarReadyToFinishRentMessage msg, CancellationToken ct = default)
    {
        await _rentService.IssueInvoiceToFinishRentAsync(msg.UserId, msg.RentId, msg.Car, ct);
    }
}