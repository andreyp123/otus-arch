using System;
using System.Threading.Tasks;
using CarSvc.Dal.Repositories;
using Common.Events;
using Common.Events.Messages;
using Common.Events.Producer;
using Common.Model.CarSvc;
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

    public CarStateController(ILogger<CarStateController> logger, ICarRepository repository, IEventProducer eventProducer)
    {
        _logger = logger;
        _repository = repository;
        _eventProducer = eventProducer;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<CarDto> GetCar()
    {
        // todo: get current carId
        var carId = "";
        
        var car = await _repository.GetCarAsync(carId, HttpContext.RequestAborted);
        return new CarDto
        {
            CarId = car.CarId,
            Brand = car.Brand,
            Model = car.Model,
            Color = car.Color,
            ReleaseDate = car.ReleaseDate,
            BodyStyle = car.BodyStyle.ToString(),
            DoorsCount = car.DoorsCount,
            Transmission = car.Transmission.ToString(),
            FuelType = car.FuelType.ToString(),
            PricePerHour = car.PricePerHour,
            PricePerKm = car.PricePerKm,
            DriveState = car.DriveState.ToString(),
            Mileage = car.Mileage,
            LocationLat = car.LocationLat,
            LocationLon = car.LocationLon,
            RemainingFuel = car.RemainingFuel,
            Alert = car.Alert,
            CreatedDate = car.CreatedDate,
            ModifiedDate = car.ModifiedDate
        };
    }

    [HttpPut]
    [Authorize]
    public async Task UpdateCarState(CarStateDto carState)
    {
        // todo: get current carId
        var carId = "";

        var ct = HttpContext.RequestAborted;
        var state = new CarState
        {
            DriveState = carState.DriveState != null ? Enum.Parse<CarDriveState>(carState.DriveState) : null,
            Mileage = carState.Mileage,
            LocationLat = carState.LocationLat,
            LocationLon = carState.LocationLon,
            RemainingFuel = carState.LocationLon,
            Alert = carState.Alert
        };
        
        await _repository.UpdateCarStateAsync(carId, state, ct);

        await _eventProducer.ProduceEventAsync(Topics.Cars, PrepareCarStateEvent(carId, state), ct);
    }

    private ProducedEvent<CarStateMessage> PrepareCarStateEvent(string carId, CarState carState)
    {
        return new()
        {
            Type = EventType.CarStateUpdated,
            Payload = new CarStateMessage
            {
                CarId = carId,
                DriveState = carState.DriveState,
                Mileage = carState.Mileage
            }
        };
    }
}