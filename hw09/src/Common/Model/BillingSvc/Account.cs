namespace Common.Model.BillingSvc
{
    public class Account
    {
        public string? AccountId { get; set; }
        public string? UserId { get; set; }
        public string? Currency { get; set; }
        public decimal Balance { get; set; } = 0;
        public DateTime CreatedDate { get; set; }
    }
}
