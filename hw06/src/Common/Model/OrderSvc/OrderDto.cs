namespace Common.Model.OrderSvc
{
    public class OrderDto
    {
        public string? OrderId { get; set; }
        public decimal Amount { get; set; } = 0;
        public string? Data { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? State { get; set; }
    }
}
