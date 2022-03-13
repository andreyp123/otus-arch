using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Common.Helpers;
using Common.Model.NotificationSvc;
using Microsoft.Extensions.Logging;
using NotificationSvc.Api.Email;
using NotificationSvc.Dal.Repositories;

namespace NotificationSvc.Api.EventHandlers;

public class NotificationEventHandler : EventHandlerBase<NotificationMessage>
{
    private readonly ILogger<NotificationEventHandler> _logger;
    private readonly INotificationRepository _repository;
    private readonly IEmailSender _emailSender;

    public NotificationEventHandler(ILogger<NotificationEventHandler> logger, INotificationRepository repository,
        IEmailSender emailSender)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _emailSender = emailSender;
    }
    
    public override string Topic => Topics.Notifications;
    public override string EventType => EventTypes.Notification;
    
    protected override async Task HandleMessageAsync(NotificationMessage msg, CancellationToken ct = default)
    {
        await _repository.CreateNotificationAsync(
            new Notification
            {
                NotificationId = Generator.GenerateId(),
                UserId = msg.UserId,
                Data = msg.Data,
                CreatedDate = DateTime.UtcNow
            }, ct);
        
        _logger.LogInformation("Saved notification into DB");

        if (!string.IsNullOrEmpty(msg.UserEmail) && !string.IsNullOrEmpty(msg.Data))
        {
            await _emailSender.SendAsync(
                new Email.Email
                {
                    To = msg.UserEmail,
                    Subject = "Crash notification",
                    Body = msg.Data,
                    IsBodyHtml = false
                }, ct);
        }
    }
}