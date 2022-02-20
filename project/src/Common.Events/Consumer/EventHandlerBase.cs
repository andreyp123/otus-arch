using Common.Helpers;
using Microsoft.Extensions.Logging;

namespace Common.Events.Consumer;

public abstract class EventHandlerBase<TMessage> : IEventHandler
{
    private readonly ILogger<EventHandlerBase<TMessage>> _logger;

    protected EventHandlerBase(ILogger<EventHandlerBase<TMessage>> logger)
    {
        _logger = logger;
    }

    public abstract string Topic { get; }
    
    public abstract string EventType { get; }

    public virtual async Task HandleEventAsync(string rawEvent, CancellationToken ct = default)
    {
        _logger.LogInformation($"Handling event {Topic}/{EventType}...");
        
        var msg = JsonHelper.Deserialize<TMessage>(rawEvent);
        if (msg == null)
        {
            throw new CrashException($"Unable to deserialize {nameof(TMessage)}");
        }

        await HandleMessageAsync(msg, ct);
        
        _logger.LogInformation($"Handled event {Topic}/{EventType}");
    }

    protected abstract Task HandleMessageAsync(TMessage msg, CancellationToken ct = default);
}