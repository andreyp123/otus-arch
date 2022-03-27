using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class PaymentPerformedEventHandler : EventHandlerBase<PaymentPerformedMessage>
{
    private readonly ILogger<PaymentPerformedEventHandler> _logger;
    private readonly IRentService _rentService;
    
    public PaymentPerformedEventHandler(ILogger<PaymentPerformedEventHandler> logger, IRentService rentSrvice)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentSrvice;
    }

    public override string Topic => Topics.Billing;
    public override string EventType => EventTypes.PaymentPerformed;
    
    protected override async Task HandleMessageAsync(PaymentPerformedMessage msg, CancellationToken ct = default)
    {
        await _rentService.CompleteRentFinishAsync(msg.UserId, msg.RentId, msg.TracingContext, ct);
    }
}