using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Microsoft.Extensions.Logging;
using RentSvc.Api.Service;

namespace RentSvc.Api.EventHandlers;

public class UserUpdatedEventHandler : EventHandlerBase<UserUpdatedMessage>
{
    private ILogger<UserUpdatedEventHandler> _logger;
    private readonly IRentService _rentService;
    
    public UserUpdatedEventHandler(ILogger<UserUpdatedEventHandler> logger, IRentService rentService)
        : base(logger)
    {
        _logger = logger;
        _rentService = rentService;
    }

    public override string Topic => Topics.Users;
    public override string EventType => EventTypes.UserUpdated;
    
    protected override async Task HandleMessageAsync(UserUpdatedMessage msg, CancellationToken ct = default)
    {
        await _rentService.UpdateUserAsync(msg.UserId, msg.User, msg.DeletedDate, msg.TracingContext, ct);
    }
}