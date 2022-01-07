using System.Threading;
using System.Threading.Tasks;
using Common.Model.OrderSvc;

namespace OrderSvc.Repository
{
    public interface IOrderRepository
    {
        Task<string> CreateOrderAsync(Order order, CancellationToken ct);
        Task<Order> GetOrderAsync(string orderId, CancellationToken ct);
        Task<(Order[], int)> GetUserActiveOrdersAsync(string userId, int start, int size, CancellationToken ct);
        Task CancelOrderAsync(string orderId, CancellationToken ct);
    }
}
