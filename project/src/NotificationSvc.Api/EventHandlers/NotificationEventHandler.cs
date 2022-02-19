using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Events;
using Common.Events.Messages;
using Common.Helpers;
using Common.Model.NotificationSvc;
using Microsoft.Extensions.Logging;
using NotificationSvc.Dal.Repositories;

namespace NotificationSvc.Api.EventHandlers;

public class NotificationEventHandler : IEventHandler
{
    private readonly ILogger<NotificationEventHandler> _logger;
    private readonly INotificationRepository _repository;

    public NotificationEventHandler(ILogger<NotificationEventHandler> logger, INotificationRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
    
    public string Topic => Topics.Notifications;

    public async Task HandleEventAsync(ConsumedEvent ev, CancellationToken ct = default)
    {
        _logger.LogInformation($"Handling event from '{Topic}'...");

        if (ev.Type == EventType.Notification)
        {
            var ntfMessage = JsonHelper.Deserialize<NotificationMessage>(ev.Payload);
            if (ntfMessage == null)
            {
                throw new CrashException("Unable to deserialize notification message");
            }

            await _repository.CreateNotificationAsync(
                new Notification
                {
                    NotificationId = Generator.GenerateId(),
                    UserId = ntfMessage.UserId,
                    Data = ntfMessage.Data,
                    CreatedDate = DateTime.UtcNow
                }, ct);
        
            _logger.LogInformation("Handled notification event. Saved notification message into DB");
        }
        else
        {
            _logger.LogInformation($"Skipped event {ev.Type}");
        }
    }
}