namespace Common.Events.Consumer;

public interface IEventHandler
{
    string Topic { get; }
    string EventType { get; }
    Task HandleEventAsync(string rawEvent, CancellationToken ct = default);
}