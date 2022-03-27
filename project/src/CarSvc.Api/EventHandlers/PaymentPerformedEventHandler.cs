using System;
using System.Threading;
using System.Threading.Tasks;
using CarSvc.Dal.Repositories;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Common.Tracing;
using Microsoft.Extensions.Logging;

namespace CarSvc.Api.EventHandlers;

public class PaymentPerformedEventHandler : EventHandlerBase<PaymentPerformedMessage>
{
    private readonly ILogger<PaymentPerformedEventHandler> _logger;
    private readonly ICarRepository _repository;
    private readonly ITracer _tracer;
    
    public PaymentPerformedEventHandler(ILogger<PaymentPerformedEventHandler> logger, ICarRepository repository, ITracer tracer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _tracer = tracer;
    }

    public override string Topic => Topics.Billing;
    public override string EventType => EventTypes.PaymentPerformed;
    
    protected override async Task HandleMessageAsync(PaymentPerformedMessage msg, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("HandlePaymentPerformed", msg.TracingContext);
        
        await _repository.FinishCarRent(msg.CarId, msg.RentId, DateTime.UtcNow, ct);
    }
}