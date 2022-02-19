namespace Common.Events;

public interface IEventHandler
{
    string Topic { get; }
    Task HandleEventAsync(ConsumedEvent ev, CancellationToken ct = default);
}