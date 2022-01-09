using System;

namespace OrderSvc.Repository.Model;

public class OrderEntity
{
    public int Id { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Data { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string State { get; set; }
    public string Message { get; set; }
}
