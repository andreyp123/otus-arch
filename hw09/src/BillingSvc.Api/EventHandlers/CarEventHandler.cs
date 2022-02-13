using System;
using System.Threading;
using System.Threading.Tasks;
using BillingSvc.Dal.Repositories;
using Common;
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using Microsoft.Extensions.Logging;

namespace BillingSvc.Api.EventHandlers;

public class CarEventHandler : EventHandlerBase
{
    private readonly ILogger<CarEventHandler> _logger;
    private readonly IAccountRepository _repository;
    private readonly IEventProducer _eventProducer;

    public CarEventHandler(ILogger<CarEventHandler> logger, IAccountRepository repository, IEventProducer eventProducer)
        : base(logger)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
        
        RegisterEventType(EventType.CarReserved, HandleCarReservedAsync);
    }
    
    public override string Topic => Topics.Cars;

    private async Task HandleCarReservedAsync(ConsumedEvent ev, CancellationToken ct)
    {
        await HandleWithPayload(ev, async (CarReservedMessage message) =>
        {
            try
            {
                var account = await _repository.GetAccountAsync(message.UserId, ct);
                if (account.Balance <= 0)
                {
                    throw new CrashException("Balance is not positive");
                }
                
                await _eventProducer.ProduceEventAsync(Topics.Billing, new ProducedEvent<AccountAuthorizedMessage>
                {
                    Type = EventType.AccountAuthorized,
                    Payload = new AccountAuthorizedMessage
                    {
                        RentId = message.RentId,
                        CarId = message.Car?.CarId,
                        UserId = message.UserId
                    }
                }, ct);
            }
            catch (Exception ex)
            {
                await _eventProducer.ProduceEventAsync(Topics.Billing, new ProducedEvent<AccountAuthorizationFailedMessage>
                {
                    Type = EventType.AccountAuthorizationFailed,
                    Payload = new AccountAuthorizationFailedMessage
                    {
                        RentId = message.RentId,
                        CarId = message.Car?.CarId,
                        UserId = message.UserId,
                        Message = $"Account authorization failed. {ex.Message}"
                    }
                }, ct);
            }
        });
    }
}