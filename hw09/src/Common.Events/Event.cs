using System.Text.Json.Nodes;

namespace Common.Events;

public class ConsumedEvent
{
    public string Type { get; set; }
    public JsonObject Payload { get; set; }
}

public class ProducedEvent<TPayload>
{
    public string Type { get; set; }
    public TPayload Payload { get; set; }
}