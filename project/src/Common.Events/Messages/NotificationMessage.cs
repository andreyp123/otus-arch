namespace Common.Events.Messages;

public class NotificationMessage : MessageBase
{
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Data { get; set; }
}