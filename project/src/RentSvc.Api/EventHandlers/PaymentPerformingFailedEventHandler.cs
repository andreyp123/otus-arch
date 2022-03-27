using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class PaymentPerformingFailedEventHandler : EventHandlerBase<PaymentPerformingFailedMessage>
{
    private readonly ILogger<PaymentPerformingFailedEventHandler> _logger;
    private readonly IRentService _rentService;
    
    public PaymentPerformingFailedEventHandler(ILogger<PaymentPerformingFailedEventHandler> logger, IRentService rentService)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentService;
    }

    public override string Topic => Topics.Billing;
    public override string EventType => EventTypes.PaymentPerformingFailed;
    
    protected override async Task HandleMessageAsync(PaymentPerformingFailedMessage msg, CancellationToken ct = default)
    {
        await _rentService.FailRentFinishAsync(msg.UserId, msg.RentId, msg.Message, msg.TracingContext, ct);
    }
}