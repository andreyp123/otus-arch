using System;
using System.Threading;
using System.Threading.Tasks;
using CarSvc.Dal.Repositories;
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using Microsoft.Extensions.Logging;

namespace CarSvc.Api.EventHandlers;

public class BillingEventHandler : EventHandlerBase
{
    private readonly ILogger<BillingEventHandler> _logger;
    private readonly ICarRepository _repository;
    private readonly IEventProducer _eventProducer;

    public BillingEventHandler(ILogger<BillingEventHandler> logger, ICarRepository repository, IEventProducer eventProducer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;

        RegisterEventType(EventType.AccountAuthorized, HandleAccountAuthorizedAsync);
        RegisterEventType(EventType.AccountAuthorizationFailed, HandleAccountAuthorizationFailedAsync);
    }

    public override string Topic => Topics.Billing;

    private Task HandleAccountAuthorizedAsync(ConsumedEvent ev, CancellationToken ct)
    {
        // todo: do nothing?
        //await HandleWithPayload(ev, async (AccountAuthorizedMessage message) => { });
        
        return Task.CompletedTask;
    }

    private async Task HandleAccountAuthorizationFailedAsync(ConsumedEvent ev, CancellationToken ct)
    {
        await HandleWithPayload(ev, async (AccountAuthorizationFailedMessage message) =>
        {
            await _repository.FinishCarRent(message.CarId, message.RentId, DateTime.UtcNow, ct);
        });
    }
}