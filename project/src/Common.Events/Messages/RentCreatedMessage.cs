namespace Common.Events.Messages;

public class RentCreatedMessage
{
    public string RentId { get; set; }
    public string CarId { get; set; }
    public string UserId { get; set; }
}