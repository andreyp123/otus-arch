namespace Common.Events.Messages;

public class RentInvoiceCreatedMessage
{
    public string RentId { get; set; }
    public string CarId { get; set; }
    public string UserId { get; set; }
    public decimal Amount { get; set; }
    public string Message { get; set; }
}