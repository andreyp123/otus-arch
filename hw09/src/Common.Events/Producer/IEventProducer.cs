namespace Common.Events.Producer;

public interface IEventProducer
{
    Task ProduceEventAsync<TPayload>(string topic, ProducedEvent<TPayload> ev, CancellationToken ct = default);
}