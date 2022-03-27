using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CarSvc.Api.Helpers;
using CarSvc.Dal.Repositories;
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Model.CarSvc;
using Common.Tracing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarSvc.Api.Controllers;

[ApiController]
[Route("carstate")]
public class CarStateController : ControllerBase
{
    private readonly ILogger<CarStateController> _logger;
    private readonly ICarRepository _repository;
    private readonly IEventProducer _eventProducer;
    private readonly ITracer _tracer;

    public CarStateController(ILogger<CarStateController> logger, ICarRepository repository, IEventProducer eventProducer,
        ITracer tracer)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
        _tracer = tracer;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<CarDto> GetCar()
    {
        var carId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;;
        
        var car = await _repository.GetCarAsync(carId, HttpContext.RequestAborted);
        return CarMapper.MapCarDto(car);
    }

    [HttpPut]
    [Authorize]
    public async Task UpdateCarState(CarStateDto carState)
    {
        var carId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        using var activity = _tracer.StartActivity("UpdateCarState");
        
        var ct = HttpContext.RequestAborted;
        var state = new CarState
        {
            DriveState = carState.DriveState != null ? Enum.Parse<CarDriveState>(carState.DriveState) : null,
            Mileage = carState.Mileage,
            LocationLat = carState.LocationLat,
            LocationLon = carState.LocationLon,
            RemainingFuel = carState.RemainingFuel,
            Alert = carState.Alert
        };
        
        await _repository.UpdateCarStateAsync(carId, state, ct);
        
        ProduceCarStateEvent(carId, state, activity);
    }

    private void ProduceCarStateEvent(string carId, CarState state, Activity activity)
    {
        _eventProducer.ProduceEventAsync(
                new EventKey(Topics.Cars, EventTypes.CarStateUpdated),
                new CarStateUpdatedMessage
                {
                    CarId = carId,
                    DriveState = state.DriveState.ToString(),
                    Mileage = state.Mileage,
                    TracingContext = _tracer.GetContext(activity)
                },
                new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token)
            .ContinueWith(
                t => _logger.LogError(t.Exception, "Error while producing car state event"),
                TaskContinuationOptions.OnlyOnFaulted);
    }
}