using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using BillingSvc.Dal.Repositories;
using Common;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Tracing;
using Microsoft.Extensions.Logging;

namespace BillingSvc.Api.EventHandlers;

public class RentInvoiceCreatedEventHandler : EventHandlerBase<RentInvoiceCreatedMessage>
{
    private ILogger<RentInvoiceCreatedEventHandler> _logger;
    private readonly IAccountRepository _repository;
    private readonly IEventProducer _eventProducer;
    private readonly ITracer _tracer;
    
    public RentInvoiceCreatedEventHandler(ILogger<RentInvoiceCreatedEventHandler> logger, IAccountRepository repository,
        IEventProducer eventProducer, ITracer tracer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
        _tracer = tracer;
    }

    public override string Topic => Topics.Rents;
    public override string EventType => EventTypes.RentInvoiceCreated;
    
    protected override async Task HandleMessageAsync(RentInvoiceCreatedMessage msg, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("HandleRentInvoiceCreated", msg.TracingContext);
        
        try
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var account = await _repository.GetAccountAsync(msg.UserId, ct);
            if (account.Balance < msg.Amount)
            {
                throw new CrashException("Not enough money");
            }

            await _repository.UpdateBalanceAsync(msg.UserId, -msg.Amount, ct);

            await _eventProducer.ProduceEventAsync(
                new EventKey(Topics.Billing, EventTypes.PaymentPerformed),
                new PaymentPerformedMessage
                {
                    RentId = msg.RentId,
                    CarId = msg.CarId,
                    UserId = msg.UserId,
                    TracingContext = _tracer.GetContext(activity)
                }, ct);
            
            scope.Complete();
        }
        catch (Exception ex)
        {
            await _eventProducer.ProduceEventAsync(
                new EventKey(Topics.Billing, EventTypes.PaymentPerformingFailed),
                new PaymentPerformingFailedMessage
                {
                    RentId = msg.RentId,
                    CarId = msg.CarId,
                    UserId = msg.UserId,
                    Message = $"Payment performing failed. {ex.Message}",
                    TracingContext = _tracer.GetContext(activity)
                }, ct);
        }
    }
}