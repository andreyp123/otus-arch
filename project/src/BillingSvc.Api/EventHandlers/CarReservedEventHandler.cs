using System;
using System.Threading;
using System.Threading.Tasks;
using BillingSvc.Dal.Repositories;
using Common;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Tracing;
using Microsoft.Extensions.Logging;

namespace BillingSvc.Api.EventHandlers;

public class CarReservedEventHandler : EventHandlerBase<CarReservedMessage>
{
    private ILogger<CarReservedEventHandler> _logger;
    private readonly IAccountRepository _repository;
    private readonly IEventProducer _eventProducer;
    private readonly ITracer _tracer;
    
    public CarReservedEventHandler(ILogger<CarReservedEventHandler> logger, IAccountRepository repository,
        IEventProducer eventProducer, ITracer tracer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
        _tracer = tracer;
    }

    public override string Topic => Topics.Cars;
    public override string EventType => EventTypes.CarReserved;
    
    protected override async Task HandleMessageAsync(CarReservedMessage msg, CancellationToken ct = default)
    {
        using var activity = _tracer.StartActivity("HandleCarReserved", msg.TracingContext);
        
        try
        {
            var account = await _repository.GetAccountAsync(msg.UserId, ct);
            if (account.Balance <= 0)
            {
                throw new CrashException("Balance is not positive");
            }
                
            await _eventProducer.ProduceEventAsync(
                new EventKey(Topics.Billing, EventTypes.AccountAuthorized),
                new AccountAuthorizedMessage
                {
                    RentId = msg.RentId,
                    CarId = msg.Car?.CarId,
                    UserId = msg.UserId,
                    TracingContext = _tracer.GetContext(activity)
                }, ct);
        }
        catch (Exception ex)
        {
            await _eventProducer.ProduceEventAsync(
                new EventKey(Topics.Billing, EventTypes.AccountAuthorizationFailed),
                new AccountAuthorizationFailedMessage
                {
                    RentId = msg.RentId,
                    CarId = msg.Car?.CarId,
                    UserId = msg.UserId,
                    Message = $"Account authorization failed. {ex.Message}",
                    TracingContext = _tracer.GetContext(activity)
                }, ct);
        }
    }
}