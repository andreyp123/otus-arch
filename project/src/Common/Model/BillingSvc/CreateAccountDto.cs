namespace Common.Model.BillingSvc
{
    public class CreateAccountDto
    {
        public string? Currency { get; set; }
        public decimal Balance { get; set; }
    }
}
