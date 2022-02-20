using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using Microsoft.Extensions.Logging;

namespace RentSvc.Api.Extensions;

public static class EventProducerExtension
{
    public static void ProduceNotification(this IEventProducer eventProducer, NotificationMessage message, ILogger logger)
    {
        eventProducer.ProduceEventAsync(
                new EventKey(Topics.Notifications, EventTypes.Notification),
                message,
                new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token)
            .ContinueWith(
                t => logger.LogError(t.Exception, "Error while producing notification event"),
                TaskContinuationOptions.OnlyOnFaulted);
    }
}