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

public class AccountAuthorizedEventHandler : EventHandlerBase<AccountAuthorizedMessage>
{
    private ILogger<AccountAuthorizedEventHandler> _logger;
    private readonly IRentRepository _repository;
    private readonly IEventProducer _eventProducer;
    
    public AccountAuthorizedEventHandler(ILogger<AccountAuthorizedEventHandler> logger, IRentRepository repository, IEventProducer eventProducer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
    }

    public override string Topic => Topics.Billing;
    public override string EventType => EventTypes.AccountAuthorized;
    
    protected override async Task HandleMessageAsync(AccountAuthorizedMessage msg, CancellationToken ct = default)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var rent = await _repository.GetRentAsync(msg.RentId, ct);
        if (rent.State != RentState.Starting)
        {
            _logger.LogError($"Rent wrong state (expected {RentState.Starting}, actual {rent.State}). Doing nothing");
            return;
        }
            
        // update rent
        rent.StartDate = DateTime.UtcNow;
        rent.State = RentState.Started;
        rent.Message = "Rent is started";
        await _repository.UpdateRentAsync(msg.RentId, rent, ct);
        
        scope.Complete();
            
        // send notification
        _eventProducer.ProduceNotification(
            new NotificationMessage
            {
                UserId = rent.UserId,
                Data = "Rent is started successfully"
            },
            _logger);
    }
}