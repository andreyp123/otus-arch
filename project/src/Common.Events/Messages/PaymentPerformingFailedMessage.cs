namespace Common.Events.Messages;

public class PaymentPerformingFailedMessage
{
    public string RentId { get; set; }
    public string CarId { get; set; }
    public string UserId { get; set; }
    public string AccountId { get; set; }
    public string Message { get; set; }
}