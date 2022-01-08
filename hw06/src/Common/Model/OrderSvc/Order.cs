namespace Common.Model.OrderSvc
{
    public class Order
    {
        public string? OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? Data { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public OrderState State { get; set; }
        public string Message { get; set; }
    }
}
