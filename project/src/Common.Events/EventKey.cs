namespace Common.Events;

public readonly struct EventKey
{
    public string Topic { get; }
    public string EventType { get; }

    public EventKey(string topic, string eventType)
    {
        Topic = topic;
        EventType = eventType;
    }

    public override bool Equals(object? obj)
    {
        return obj is EventKey other &&
               Topic == other.Topic &&
               EventType == other.EventType;
    }

    public override int GetHashCode()
    {
        int hashcode = 13;
        hashcode = hashcode * 7 ^ (Topic != null ? Topic.GetHashCode() : 0);
        hashcode = hashcode * 7 ^ (EventType != null ? EventType.GetHashCode() : 0);
        return hashcode;
    }
}