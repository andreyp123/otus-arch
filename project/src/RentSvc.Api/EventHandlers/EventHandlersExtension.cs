using Common.Events.Consumer;
using Microsoft.Extensions.DependencyInjection;

namespace RentSvc.Api.EventHandlers;

public static class EventHandlersExtension
{
    public static IServiceCollection AddEventHandling(this IServiceCollection services)
    {
        services.AddEventConsumer();
        services.AddSingleton<IEventHandler, AccountAuthorizedEventHandler>();
        services.AddSingleton<IEventHandler, AccountAuthorizationFailedEventHandler>();
        services.AddSingleton<IEventHandler, CarStateUpdatedEventHandler>();
        services.AddSingleton<IEventHandler, CarReservedEventHandler>();
        services.AddSingleton<IEventHandler, CarReservationFailedEventHandler>();
        return services;
    }
}