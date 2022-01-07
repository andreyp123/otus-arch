using Common;
using Common.Helpers;
using Common.Model.OrderSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OrderSvc.Repository;
using Common.Model;
using System;

namespace OrderSvc.Api.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderRepository _repository;

        public OrderController(ILogger<OrderController> logger, IOrderRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpPost]
        [Authorize]
        public async Task<string> CreateOrder([FromBody] CreateOrderDto ord)
        {
            Guard.NotNull(ord, nameof(ord));

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            return await _repository.CreateOrderAsync(
                new Order
                {
                    OrderId = IdGenerator.Generate(),
                    UserId = userId,
                    Amount = ord.Amount,
                    Data = ord.Data,
                    State = OrderState.Pending,
                    CreatedDate = DateTime.UtcNow
                },
                HttpContext.RequestAborted);
        }

        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<OrderDto> GetOrder(string orderId)
        {
            var order = await _repository.GetOrderAsync(orderId, HttpContext.RequestAborted);
            return MapOrderDto(order);
        }

        [HttpGet]
        [Authorize]
        public async Task<ListResult<OrderDto>> GetUserActiveOrders([FromQuery] int start, [FromQuery] int size)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            (Order[] users, int total) = await _repository.GetUserActiveOrdersAsync(userId, start, size, HttpContext.RequestAborted);
            return new ListResult<OrderDto>(
                users.Select(o => MapOrderDto(o)).ToArray(),
                total);
        }

        [HttpPost("{orderId}/cancel")]
        [Authorize]
        public async Task CancelOrder(string orderId)
        {
            await _repository.CancelOrderAsync(orderId, HttpContext.RequestAborted);
        }

        private OrderDto MapOrderDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                Amount = order.Amount,
                Data = order.Data,
                State = order.State.ToString(),
                CreatedDate = order.CreatedDate
            };
        }
    }
}
