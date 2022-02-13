using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Model.RentSvc;
using Microsoft.Extensions.Logging;
using RentSvc.Dal.Repositories;

namespace RentSvc.Api.EventHandlers;

public class BillingEventHandler : EventHandlerBase
{
    private readonly ILogger<BillingEventHandler> _logger;
    private readonly IRentRepository _repository;
    private readonly IEventProducer _eventProducer;

    public BillingEventHandler(ILogger<BillingEventHandler> logger, IRentRepository repository, IEventProducer eventProducer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;

        RegisterEventType(EventType.AccountAuthorized, HandleAccountAuthorizedAsync);
        RegisterEventType(EventType.AccountAuthorizationFailed, HandleAccountAuthorizationFailedAsync);
    }

    public override string Topic => Topics.Billing;

    private async Task HandleAccountAuthorizedAsync(ConsumedEvent ev, CancellationToken ct)
    {
        await HandleWithPayload(ev,  async (AccountAuthorizedMessage message) =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var rent = await _repository.GetRentAsync(message.RentId, ct);
            if (rent.State != RentState.Starting)
            {
                _logger.LogError($"Rent wrong state (expected {RentState.Starting}, actual {rent.State}). Doing nothing");
                return;
            }
            
            // update rent
            rent.StartDate = DateTime.UtcNow;
            rent.State = RentState.Started;
            rent.Message = "Rent is started";
            await _repository.UpdateRentAsync(message.RentId, rent, ct);
            
            // send notification
            await _eventProducer.ProduceEventAsync(Topics.Notifications, new ProducedEvent<NotificationMessage>
            {
                Type = EventType.Notification,
                Payload = new NotificationMessage
                {
                    UserId = rent.UserId,
                    Data = "Rent is started successfully"
                }
            }, ct);

            scope.Complete();
        });
    }

    private async Task HandleAccountAuthorizationFailedAsync(ConsumedEvent ev, CancellationToken ct)
    {
        await HandleWithPayload(ev, async (AccountAuthorizationFailedMessage message) =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var rent = await _repository.GetRentAsync(message.RentId, ct);
            if (rent.State != RentState.Starting)
            {
                _logger.LogError($"Rent wrong state (expected {RentState.Starting}, actual {rent.State}). Doing nothing");
                return;
            }

            // update rent
            rent.EndDate = DateTime.UtcNow;
            rent.State = RentState.Error;
            rent.Message = $"Rent starting failed. {message.Message}";
            await _repository.UpdateRentAsync(message.RentId, rent, ct);

            // send notification
            await _eventProducer.ProduceEventAsync(Topics.Notifications, new ProducedEvent<NotificationMessage>
            {
                Type = EventType.Notification,
                Payload = new NotificationMessage
                {
                    UserId = rent.UserId,
                    Data = $"Rent is not started. {rent.Message}"
                }
            }, ct);

            scope.Complete();
        });
    }
}