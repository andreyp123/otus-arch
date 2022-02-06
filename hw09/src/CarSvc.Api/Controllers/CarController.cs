using System;
using Common.Model.CarSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Model;
using CarSvc.Dal.Repositories;
using Common;
using Common.Helpers;
using Common.Model.UserSvc;

namespace CarSvc.Api.Controllers
{
    [ApiController]
    [Route("cars")]
    public class CarController : ControllerBase
    {
        private readonly ILogger<CarController> _logger;
        private readonly ICarRepository _repository;

        public CarController(ILogger<CarController> logger, ICarRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Employee)]
        public async Task<string> CreateCar([FromBody] CreateCarDto car)
        {
            Guard.NotNull(car, nameof(car));
            
            DateTime now = DateTime.UtcNow;

            return await _repository.CreateCarAsync(
                new Car
                {
                    CarId = IdGenerator.Generate(),
                    Brand = car.Brand,
                    Model = car.Model,
                    Color = car.Color,
                    ReleaseDate = car.ReleaseDate,
                    BodyStyle = Enum.Parse<CarBodyStyles>(car.BodyStyle),
                    DoorsCount = car.DoorsCount,
                    Transmission = Enum.Parse<CarTransmissionTypes>(car.Transmission),
                    FuelType = Enum.Parse<CarFuelTypes>(car.FuelType),
                    PricePerHour = car.PricePerHour,
                    PricePerKm = car.PricePerKm,
                    CreatedDate = now,
                    ModifiedDate = now
                },
                HttpContext.RequestAborted);
        }

        [HttpGet]
        [Authorize]
        public async Task<ListResult<CarDto>> GetCars([FromQuery] int start, [FromQuery] int size)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            (Car[] cars, int total) = await _repository.GetCarsAsync(start, size, HttpContext.RequestAborted);
            return new ListResult<CarDto>(
                cars.Select(c => new CarDto
                    {
                        CarId = c.CarId,
                        Brand = c.Brand,
                        Model = c.Model,
                        Color = c.Color,
                        ReleaseDate = c.ReleaseDate,
                        BodyStyle = c.BodyStyle.ToString(),
                        DoorsCount = c.DoorsCount,
                        Transmission = c.Transmission.ToString(),
                        FuelType = c.FuelType.ToString(),
                        PricePerHour = c.PricePerHour,
                        PricePerKm = c.PricePerKm,
                        Mileage = c.Mileage,
                        LocationLat = c.LocationLat,
                        LocationLon = c.LocationLon,
                        RemainingFuel = c.RemainingFuel,
                        Alert = c.Alert,
                        CreatedDate = c.CreatedDate,
                        ModifiedDate = c.ModifiedDate
                    }).ToArray(),
                total);
        }
    }
}
