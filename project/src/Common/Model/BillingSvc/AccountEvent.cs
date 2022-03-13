namespace Common.Model.BillingSvc;

public class AccountEvent
{
    public string AccountId { get; set; }
    public DateTime EventDate { get; set; }
    public string? EventMessage { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
}