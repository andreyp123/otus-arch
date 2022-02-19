namespace BillingSvc.Dal.Model
{
    public class AccountEntity
    {
        public int Id { get; set; }
        public string AccountId { get; set; }
        public string UserId { get; set; }
        public string? Currency { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
