using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class AccountAuthorizedEventHandler : EventHandlerBase<AccountAuthorizedMessage>
{
    private readonly ILogger<AccountAuthorizedEventHandler> _logger;
    private readonly IRentService _rentService;
    
    public AccountAuthorizedEventHandler(ILogger<AccountAuthorizedEventHandler> logger, IRentService rentService)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentService;
    }

    public override string Topic => Topics.Billing;
    public override string EventType => EventTypes.AccountAuthorized;

    protected override async Task HandleMessageAsync(AccountAuthorizedMessage msg, CancellationToken ct = default)
    {
        await _rentService.CompleteRentStartAsync(msg.UserId, msg.RentId, ct);
    }
}