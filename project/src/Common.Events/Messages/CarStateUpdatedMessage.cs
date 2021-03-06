namespace Common.Events.Messages;

public class CarStateUpdatedMessage : MessageBase
{
    public string CarId { get; set; }
    public string? DriveState { get; set; }
    public int? Mileage { get; set; }
}