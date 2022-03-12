namespace Common.Events.Messages;

public class PaymentPerformedMessage
{
    public string RentId { get; set; }
    public string CarId { get; set; }
    public string UserId { get; set; }
}