namespace Common.Events.Producer;

public interface IEventProducer
{
    Task ProduceEventAsync<TEvent>(EventKey ek, TEvent ev, CancellationToken ct = default);
}