namespace Common.Events.Messages;

public class CarReservationFailedMessage : MessageBase
{
    public string RentId { get; set; }
    public string CarId { get; set; }
    public string UserId { get; set; }
    public string Message { get; set; }
}