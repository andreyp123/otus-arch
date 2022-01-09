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
using System.Threading;
using System.Transactions;
using Common.Model.NotificationSvc;
using Microsoft.AspNetCore.Authentication;
using OrderSvc.Api.BillingClient;
using OrderSvc.Api.KafkaProducer;

namespace OrderSvc.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrderController : ControllerBase
{
    private const string NOTIFICATIONS_TOPIC = "notifications";
    
    private readonly ILogger<OrderController> _logger;
    private readonly IOrderRepository _repository;
    private readonly IRequestRepository _reqRepository;
    private readonly IBillingClient _billing;
    private readonly IKafkaProducer _kafkaProducer;

    public OrderController(ILogger<OrderController> logger, IOrderRepository repository,
        IRequestRepository reqRepository, IBillingClient billing, IKafkaProducer kafkaProducer)
    {
        _logger = logger;
        _repository = repository;
        _reqRepository = reqRepository;
        _billing = billing;
        _kafkaProducer = kafkaProducer;
    }

    [HttpPost]
    [Authorize]
    public async Task<string> CreateOrder(
        [FromHeader(Name = "Idempotence-Key")] string idempotenceKey,
        [FromBody] CreateOrderDto ord)
    {
        Guard.NotNull(ord, nameof(ord));
        
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        CancellationToken ct = HttpContext.RequestAborted;
        DateTime now = DateTime.UtcNow;
        
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // check request for idempotency
        if (!string.IsNullOrEmpty(idempotenceKey) &&
            !await _reqRepository.CheckCreateRequestAsync(idempotenceKey, "CreateOrder", now, ct))
        {
            throw new EShopConflictException("Idempotency error");
        }

        // create order
        var order = new Order
        {
            OrderId = IdGenerator.Generate(),
            UserId = userId,
            Amount = ord.Amount,
            Data = ord.Data,
            State = OrderState.Pending,
            CreatedDate = now,
            Message = ""
        };
        await _repository.CreateOrderAsync(order, ct);
        
        scope.Complete();
        
        // update billing
        _billing.SetToken(await HttpContext.GetTokenAsync("access_token"));
        try
        {
            var account = await _billing.WithdrawAccountAsync(ord.Amount, ct);
            _logger.LogInformation($"Withdrawn account {account.AccountId}. Balance: {account.Balance} {account.Currency}");
            order.Message = "Finished successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing account");
            order.Message = $"Finished with failure. {ex.Message}";
        }
        
        // update order
        await _repository.UpdateOrderStateAsync(order.OrderId, OrderState.Finished, order.Message, ct);
        
        // send notification
        Task.Run(() => _kafkaProducer.SendAsync(NOTIFICATIONS_TOPIC, PrepareMessage(order), ct));

        return order.OrderId;
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

        (Order[] users, int total) = await _repository.GetUserOrdersAsync(userId, start, size, HttpContext.RequestAborted);
        return new ListResult<OrderDto>(
            users.Select(MapOrderDto).ToArray(),
            total);
    }

    [HttpPost("{orderId}/cancel")]
    [Authorize]
    public async Task CancelOrder(string orderId)
    {
        var order = await _repository.GetOrderAsync(orderId, HttpContext.RequestAborted);
        if (order.State != OrderState.Pending)
        {
            throw new EShopException("Order is not active");
        }

        await _repository.UpdateOrderStateAsync(orderId, OrderState.Cancelled, "Cancelled by user", HttpContext.RequestAborted);
    }

    private OrderDto MapOrderDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            Amount = order.Amount,
            Data = order.Data,
            CreatedDate = order.CreatedDate,
            State = order.State.ToString(),
            Message = order.Message
        };
    }

    private NotificationMessage PrepareMessage(Order order)
    {
        return new NotificationMessage
        {
            UserId = order.UserId,
            Data = $"Order: {order.OrderId}. Amount: {order.Amount}. Result: {order.Message}"
        };
    }
}