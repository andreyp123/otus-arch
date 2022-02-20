using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Model.RentSvc;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Extensions;
using RentSvc.Dal.Repositories;

namespace RentSvc.Api.EventHandlers;

public class AccountAuthorizationFailedEventHandler : EventHandlerBase<AccountAuthorizationFailedMessage>
{
    private ILogger<AccountAuthorizationFailedEventHandler> _logger;
    private readonly IRentRepository _repository;
    private readonly IEventProducer _eventProducer;
    
    public AccountAuthorizationFailedEventHandler(ILogger<AccountAuthorizationFailedEventHandler> logger, IRentRepository repository, IEventProducer eventProducer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
    }

    public override string Topic => Topics.Billing;
    public override string EventType => EventTypes.AccountAuthorizationFailed;
    
    protected override async Task HandleMessageAsync(AccountAuthorizationFailedMessage msg, CancellationToken ct = default)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var rent = await _repository.GetRentAsync(msg.RentId, ct);
        if (rent.State != RentState.Starting)
        {
            _logger.LogError($"Rent wrong state (expected {RentState.Starting}, actual {rent.State}). Doing nothing");
            return;
        }

        // update rent
        rent.EndDate = DateTime.UtcNow;
        rent.State = RentState.Error;
        rent.Message = $"Rent starting failed. {msg.Message}";
        await _repository.UpdateRentAsync(msg.RentId, rent, ct);

        scope.Complete();
            
        // send notification
        _eventProducer.ProduceNotification(
            new NotificationMessage
            {
                UserId = rent.UserId,
                Data = $"Rent is not started. {rent.Message}"
            }, _logger);
    }
}