using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class AccountAuthorizationFailedEventHandler : EventHandlerBase<AccountAuthorizationFailedMessage>
{
    private readonly ILogger<AccountAuthorizationFailedEventHandler> _logger;
    private readonly IRentService _rentService;
    
    public AccountAuthorizationFailedEventHandler(ILogger<AccountAuthorizationFailedEventHandler> logger, IRentService rentService)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentService;
    }

    public override string Topic => Topics.Billing;
    public override string EventType => EventTypes.AccountAuthorizationFailed;
    
    protected override async Task HandleMessageAsync(AccountAuthorizationFailedMessage msg, CancellationToken ct = default)
    {
        await _rentService.FailRentStartAsync(msg.UserId, msg.RentId, msg.Message, ct);
    }
}