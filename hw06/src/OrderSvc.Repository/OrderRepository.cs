using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Model.OrderSvc;
using OrderSvc.Repository.Model;

namespace OrderSvc.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ILogger<OrderRepository> _logger;
        private readonly OrderDbContext _dbContext;

        public OrderRepository(ILogger<OrderRepository> logger, OrderDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<string> CreateOrderAsync(Order order, CancellationToken ct = default)
        {
            Guard.NotNull(order, nameof(order));
            Guard.NotNullOrEmpty(order.OrderId, nameof(order.OrderId));
            Guard.NotNullOrEmpty(order.UserId, nameof(order.UserId));

            if (await _dbContext.Orders.AnyAsync(oe => oe.OrderId == order.OrderId))
            {
                throw new EShopException("Order already exists");
            }
            await _dbContext.Orders.AddAsync(MapOrderEntity(order), ct);
            await _dbContext.SaveChangesAsync(ct);

            return order.OrderId;
        }

        public async Task<Order> GetOrderAsync(string orderId, CancellationToken ct = default)
        {
            var orderEntity = await _dbContext.Orders.FirstOrDefaultAsync(oe => oe.OrderId == orderId, ct);
            if (orderEntity == null)
            {
                throw new EShopException($"Order {orderId} not found");
            }
            return MapOrder(orderEntity);
        }

        public async Task<(Order[], int)> GetUserActiveOrdersAsync(string userId, int start, int size, CancellationToken ct = default)
        {
            var query = _dbContext.Orders
                .Where(oe => oe.UserId == userId && oe.State == OrderState.Pending.ToString());
            var total = await query
                .CountAsync(ct);
            var users = await query
                .Skip(start)
                .Take(size)
                .Select(oe => MapOrder(oe))
                .ToArrayAsync(ct);
            return (users, total);
        }

        public async Task CancelOrderAsync(string orderId, CancellationToken ct = default)
        {
            var orderEntity = await _dbContext.Orders.FirstOrDefaultAsync(oe => oe.OrderId == orderId, ct);
            if (orderEntity == null)
            {
                throw new EShopException($"Order {orderId} not found");
            }
            if (orderEntity.State != OrderState.Pending.ToString())
            {
                throw new EShopException($"Order {orderId} is not active");
            }

            orderEntity.State = OrderState.Cancelled.ToString();
            await _dbContext.SaveChangesAsync(ct);
        }

        private static OrderEntity MapOrderEntity(Order o)
        {
            return new OrderEntity
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                Amount = o.Amount,
                Data = o.Data,
                CreatedDate = o.CreatedDate,
                State = o.State.ToString()
            };
        }

        private static Order MapOrder(OrderEntity oe)
        {
            return new Order
            {
                OrderId = oe.OrderId,
                UserId = oe.UserId,
                Amount = oe.Amount,
                Data = oe.Data,
                CreatedDate = oe.CreatedDate,
                State = Enum.Parse<OrderState>(oe.State)
            };
        }
    }
}
