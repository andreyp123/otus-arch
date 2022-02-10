using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Events;
using Common.Events.Messages;
using Common.Helpers;
using Microsoft.Extensions.Logging;
using RentSvc.Dal.Repositories;

namespace RentSvc.Api.EventHandlers;

public class CarEventHandler : IEventHandler
{
    private readonly ILogger<CarEventHandler> _logger;
    private readonly IRentRepository _repository;

    public CarEventHandler(ILogger<CarEventHandler> logger, IRentRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
    
    public string Topic => Topics.Cars;

    public async Task HandleEventAsync(ConsumedEvent ev, CancellationToken ct = default)
    {
        _logger.LogInformation($"Handling event from '{Topic}'...");

        if (ev.Type == EventType.CarStateUpdated)
        {
            var carStateMsg = JsonHelper.Deserialize<CarStateMessage>(ev.Payload);
            if (carStateMsg == null)
            {
                throw new CrashException("Unable to deserialize car state message");
            }

            await _repository.UpdateActiveRentAsync(carStateMsg.CarId, carStateMsg.Mileage, ct);
        
            _logger.LogInformation("Handled car state event. Saved state into DB");
        }
        else
        {
            _logger.LogInformation($"Skipped event {ev.Type}");
        }
    }
}