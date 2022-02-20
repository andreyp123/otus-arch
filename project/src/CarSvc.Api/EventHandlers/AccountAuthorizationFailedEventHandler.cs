using System;
using System.Threading;
using System.Threading.Tasks;
using CarSvc.Dal.Repositories;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;

namespace CarSvc.Api.EventHandlers;

public class AccountAuthorizationFailedEventHandler : EventHandlerBase<AccountAuthorizationFailedMessage>
{
    private ILogger<AccountAuthorizationFailedEventHandler> _logger;
    private readonly ICarRepository _repository;
    
    public AccountAuthorizationFailedEventHandler(ILogger<AccountAuthorizationFailedEventHandler> logger, ICarRepository repository)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
    }

    public override string Topic => Topics.Billing;
    public override string EventType => EventTypes.AccountAuthorizationFailed;
    
    protected override async Task HandleMessageAsync(AccountAuthorizationFailedMessage msg, CancellationToken ct = default)
    {
        await _repository.FinishCarRent(msg.CarId, msg.RentId, DateTime.UtcNow, ct);
    }
}