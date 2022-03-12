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
using Microsoft.Extensions.Logging;

namespace BillingSvc.Api.EventHandlers;

public class RentInvoiceCreatedEventHandler : EventHandlerBase<RentInvoiceCreatedMessage>
{
    private ILogger<RentInvoiceCreatedEventHandler> _logger;
    private readonly IAccountRepository _repository;
    private readonly IEventProducer _eventProducer;
    
    public RentInvoiceCreatedEventHandler(ILogger<RentInvoiceCreatedEventHandler> logger, IAccountRepository repository, IEventProducer eventProducer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
    }

    public override string Topic => Topics.Rents;
    public override string EventType => EventTypes.RentInvoiceCreated;
    
    protected override async Task HandleMessageAsync(RentInvoiceCreatedMessage msg, CancellationToken ct = default)
    {
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
                    UserId = msg.UserId
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
                    Message = $"Payment performing failed. {ex.Message}"
                }, ct);
        }
    }
}