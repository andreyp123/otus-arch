using System.Threading;
using System.Threading.Tasks;
using Common.Model.OrderSvc;

namespace OrderSvc.Repository
{
    public interface IOrderRepository
    {
        Task<string> CreateOrderAsync(Order order, CancellationToken ct = default);
        Task<Order> GetOrderAsync(string orderId, CancellationToken ct = default);
        Task<(Order[], int)> GetUserOrdersAsync(string userId, int start, int size, CancellationToken ct = default);
        Task UpdateOrderStateAsync(string orderId, OrderState state, string message, CancellationToken ct = default);
    }
}
