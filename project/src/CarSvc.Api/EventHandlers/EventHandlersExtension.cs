using Common.Events.Consumer;
using Microsoft.Extensions.DependencyInjection;

namespace CarSvc.Api.EventHandlers;

public static class EventHandlersExtension
{
    public static IServiceCollection AddEventHandling(this IServiceCollection services)
    {
        services.AddEventConsumer();
        services.AddSingleton<IEventHandler, RentCreatedEventHandler>();
        services.AddSingleton<IEventHandler, AccountAuthorizationFailedEventHandler>();
        services.AddSingleton<IEventHandler, RentFinishRequestedEventHandler>();
        services.AddSingleton<IEventHandler, PaymentPerformedEventHandler>();
        return services;
    }
}