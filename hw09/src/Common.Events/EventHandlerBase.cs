using Common.Helpers;
using Microsoft.Extensions.Logging;

namespace Common.Events;

public abstract class EventHandlerBase : IEventHandler
{
    private readonly ILogger<EventHandlerBase> _logger;
    private readonly IDictionary<string, Func<ConsumedEvent, CancellationToken, Task>> _eventTypeHandlers =
        new Dictionary<string, Func<ConsumedEvent, CancellationToken, Task>>();

    protected EventHandlerBase(ILogger<EventHandlerBase> logger)
    {
        _logger = logger;
    }

    public abstract string Topic { get; }
    
    public virtual async Task HandleEventAsync(ConsumedEvent ev, CancellationToken ct = default)
    {
        _logger.LogInformation($"Handling event from '{Topic}'...");
        
        if (_eventTypeHandlers.TryGetValue(ev.Type, out var handler))
        {
            _logger.LogInformation($"Handling {ev.Type}...");

            await handler.Invoke(ev, ct);
        
            _logger.LogInformation($"Handled {ev.Type}");
        }
        else
        {
            _logger.LogInformation($"Skipped {ev.Type}");
        }
    }

    protected void RegisterEventType(string eventType, Func<ConsumedEvent, CancellationToken, Task> handler)
    {
        _eventTypeHandlers.Add(eventType, handler);
    }
    
    protected async Task HandleWithPayload<TPayload>(ConsumedEvent ev, Func<TPayload, Task> handler)
    {
        var payload = JsonHelper.Deserialize<TPayload>(ev.Payload);
        if (payload == null)
        {
            throw new CrashException($"Unable to deserialize {nameof(TPayload)}");
        }

        await handler(payload);
    }
}