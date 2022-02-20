using Common.Events.Consumer;
using Microsoft.Extensions.DependencyInjection;

namespace BillingSvc.Api.EventHandlers;

public static class EventHandlersExtension
{
    public static IServiceCollection AddEventHandling(this IServiceCollection services)
    {
        services.AddEventConsumer();
        services.AddSingleton<IEventHandler, CarReservedEventHandler>();
        return services;
    }
}