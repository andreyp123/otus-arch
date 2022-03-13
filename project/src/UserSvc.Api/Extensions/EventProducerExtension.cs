using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Model.UserSvc;
using Microsoft.Extensions.Logging;
using UserSvc.Api.Helpers;

namespace UserSvc.Api.Extensions;

public static class EventProducerExtension
{
    public static void ProduceUserUpdatedWithNoWait(this IEventProducer eventProducer, User user, ILogger logger)
    {
        ProduceUserUpdatedWithNoWaitImpl(
            eventProducer,
            new UserUpdatedMessage { UserId = user.UserId, User = UserMapper.MapUserDto(user) },
            logger);
    }
    
    public static void ProduceUserDeletedWithNoWait(this IEventProducer eventProducer, string userId, ILogger logger)
    {
        ProduceUserUpdatedWithNoWaitImpl(
            eventProducer,
            new UserUpdatedMessage { UserId = userId, User = null, DeletedDate = DateTime.UtcNow },
            logger);
    }
    
    private static void ProduceUserUpdatedWithNoWaitImpl(IEventProducer eventProducer, UserUpdatedMessage message, ILogger logger)
    {
        eventProducer.ProduceEventAsync(
                new EventKey(Topics.Users, EventTypes.UserUpdated),
                message,
                new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token)
            .ContinueWith(
                t => logger.LogError(t.Exception, "Error while producing notification event"),
                TaskContinuationOptions.OnlyOnFaulted);
    }
}