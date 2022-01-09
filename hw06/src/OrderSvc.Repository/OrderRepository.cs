using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
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

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            
            if (await _dbContext.Orders.AnyAsync(oe => oe.OrderId == order.OrderId))
            {
                throw new EShopException("Order already exists");
            }
            await _dbContext.Orders.AddAsync(MapOrderEntity(order), ct);
            await _dbContext.SaveChangesAsync(ct);

            scope.Complete();
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

        public async Task<(Order[], int)> GetUserOrdersAsync(string userId, int start, int size, CancellationToken ct = default)
        {
            var query = _dbContext.Orders
                .Where(oe => oe.UserId == userId);
            var total = await query
                .CountAsync(ct);
            var orders = await query
                .OrderByDescending(oe => oe.CreatedDate)
                .Skip(start)
                .Take(size)
                .Select(oe => MapOrder(oe))
                .ToArrayAsync(ct);
            return (orders, total);
        }

        public async Task UpdateOrderStateAsync(string orderId, OrderState state, string message = null, CancellationToken ct = default)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var orderEntity = await _dbContext.Orders.FirstOrDefaultAsync(oe => oe.OrderId == orderId, ct);
            if (orderEntity == null)
            {
                throw new EShopException($"Order {orderId} not found");
            }

            orderEntity.State = state.ToString();
            if (message != null)
            {
                orderEntity.Message = message;
            }
            await _dbContext.SaveChangesAsync(ct);
            
            scope.Complete();
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
                State = o.State.ToString(),
                Message = o.Message
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
                State = Enum.Parse<OrderState>(oe.State),
                Message = oe.Message
            };
        }
    }
}
