using System;
using System.Threading;
using System.Threading.Tasks;
using BillingSvc.Dal.Repositories;
using Common;
using Common.Events;
using Common.Events.Consumer;
using Common.Events.Messages;
using Common.Events.Producer;
using Microsoft.Extensions.Logging;

namespace BillingSvc.Api.EventHandlers;

public class CarReservedEventHandler : EventHandlerBase<CarReservedMessage>
{
    private ILogger<CarReservedEventHandler> _logger;
    private readonly IAccountRepository _repository;
    private readonly IEventProducer _eventProducer;
    
    public CarReservedEventHandler(ILogger<CarReservedEventHandler> logger, IAccountRepository repository, IEventProducer eventProducer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
    }

    public override string Topic => Topics.Cars;
    public override string EventType => EventTypes.CarReserved;
    
    protected override async Task HandleMessageAsync(CarReservedMessage msg, CancellationToken ct = default)
    {
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
                    UserId = msg.UserId
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
                    Message = $"Account authorization failed. {ex.Message}"
                }, ct);
        }
    }
}