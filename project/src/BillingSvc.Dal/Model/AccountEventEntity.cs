namespace BillingSvc.Dal.Model
{
    public class AccountEventEntity
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime EventDate { get; set; }
        public string? EventMessage { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }

        // Navigation properties
        public AccountEntity Account { get; set; }
    }
}
