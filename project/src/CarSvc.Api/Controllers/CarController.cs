using System;
using Common.Model.CarSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CarSvc.Api.Helpers;
using Common.Model;
using CarSvc.Dal.Repositories;
using Common;
using Common.Helpers;
using Common.Model.UserSvc;

namespace CarSvc.Api.Controllers;

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
    public async Task<NewCarDto> CreateCar([FromBody] CreateCarDto carToCreate)
    {
        Guard.NotNull(carToCreate, nameof(carToCreate));

        var now = DateTime.UtcNow;
        var carId = Generator.GenerateId();
        var carApiKey = Generator.GenerateApiKey();

        var car = CarMapper.MapCar(carToCreate);
        car.CarId = carId;
        car.ApiKeyHash = Hasher.CalculateHash(carApiKey);
        car.CreatedDate = now;
        car.ModifiedDate = now;
        
        await _repository.CreateCarAsync(car, HttpContext.RequestAborted);

        return new() {CarId = carId, CarApikey = carApiKey};
    }

    [HttpPost("{carId}/reset")]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task<NewCarDto> ResentCarApiKey(string carId)
    {
        Guard.NotNullOrEmpty(carId, nameof(carId));

        var ct = HttpContext.RequestAborted;
        var now = DateTime.UtcNow;
        var newApiKey = Generator.GenerateApiKey();

        var car = await _repository.GetCarAsync(carId, ct);
        car.ApiKeyHash = Hasher.CalculateHash(newApiKey);
        car.ModifiedDate = now;
        
        await _repository.UpdateCarAsync(carId, car, ct);

        return new() {CarId = carId, CarApikey = newApiKey};
    }

    [HttpGet]
    [Authorize]
    public async Task<ListResult<CarDto>> GetCars([FromQuery] int start, [FromQuery] int size)
    {
       _ = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        bool isEmpoloyee = User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == UserRoles.Employee);

        (Car[] cars, int total) = await _repository.GetCarsAsync(start, size, isEmpoloyee, HttpContext.RequestAborted);
        return new ListResult<CarDto>(
            cars.Select(CarMapper.MapCarDto).ToArray(),
            total);
    }
}