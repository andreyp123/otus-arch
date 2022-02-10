using Common.Events;
using Common.Events.Consumer;
using Microsoft.Extensions.DependencyInjection;

namespace RentSvc.Api.EventHandlers;

public static class EventHandlersExtension
{
    public static IServiceCollection AddEventHandling(this IServiceCollection services)
    {
        services.AddEventConsumer();
        services.AddSingleton<IEventHandler, CarEventHandler>();
        return services;
    }
}