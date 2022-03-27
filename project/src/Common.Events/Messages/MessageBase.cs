namespace Common.Events.Messages;

public class MessageBase
{
    public Dictionary<string, string>? TracingContext { get; set; }
}