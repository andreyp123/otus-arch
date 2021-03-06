using System.Transactions;
using Common;
using Common.Model.CarSvc;
using Microsoft.EntityFrameworkCore;
using CarSvc.Dal.Model;

namespace CarSvc.Dal.Repositories;

public class CarRepository : ICarRepository
{
    private readonly CarDalConfig _config;
    private readonly IDbContextFactory<CarDbContext> _dbContextFactory;

    public CarRepository(CarDalConfig config, IDbContextFactory<CarDbContext> dbContextFactory)
    {
        _config = config;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<string> CreateCarAsync(Car car, CancellationToken ct = default)
    {
        Guard.NotNull(car, nameof(car));

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        if (await dbContext.Cars.AnyAsync(ne => ne.CarId == car.CarId, ct))
        {
            throw new CrashException("Car already exists");
        }

        await dbContext.Cars.AddAsync(MapCarEntity(car), ct);
        await dbContext.SaveChangesAsync(ct);

        return car.CarId;
    }

    public async Task UpdateCarAsync(string carId, Car car, CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(carId, nameof(carId));
        Guard.NotNull(car, nameof(car));
        
        using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        var carEntity = await dbContext.Cars.SingleOrDefaultAsync(ce => ce.CarId == carId, ct);
        if (carEntity == null)
        {
            throw new CrashException($"Car {carId} not found");
        }

        carEntity.CarId = car.CarId;
        carEntity.Brand = car.Brand;
        carEntity.Model = car.Model;
        carEntity.Color = car.Color;
        carEntity.ReleaseDate = car.ReleaseDate;
        carEntity.BodyStyle = car.BodyStyle.ToString();
        carEntity.DoorsCount = car.DoorsCount;
        carEntity.Transmission = car.Transmission.ToString();
        carEntity.FuelType = car.FuelType.ToString();
        carEntity.PricePerHour = car.PricePerHour;
        carEntity.PricePerKm = car.PricePerKm;
        carEntity.DriveState = car.DriveState.ToString();
        carEntity.Mileage = car.Mileage;
        carEntity.LocationLat = car.LocationLat;
        carEntity.LocationLon = car.LocationLon;
        carEntity.RemainingFuel = car.RemainingFuel;
        carEntity.Alert = car.Alert;
        carEntity.ApiKeyHash = car.ApiKeyHash;
        carEntity.CreatedDate = car.CreatedDate;
        carEntity.ModifiedDate = car.ModifiedDate;

        await dbContext.SaveChangesAsync();
        tran.Complete();
    }

    public async Task<Car> GetCarAsync(string carId, CancellationToken ct = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        var carEntity = await dbContext.Cars.SingleOrDefaultAsync(ce => ce.CarId == carId, ct);
        if (carEntity == null)
        {
            throw new CrashException($"Car {carId} not found");
        }

        return MapCar(carEntity);
    }

    public async Task<(Car[], int)> GetCarsAsync(int start, int size, bool all = true, CancellationToken ct = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        var query = dbContext.Cars
            .Include(ce => ce.CarRents)
            .Where(ce => all || !ce.CarRents.Any() || ce.CarRents.All(cre => cre.RentEndDate != null));
        var total = await query
            .CountAsync(ct);
        var cars = await query
            .OrderByDescending(ce => ce.CreatedDate)
            .Skip(start)
            .Take(size)
            .Select(ce => MapCar(ce))
            .ToArrayAsync(ct);
        return (cars, total);
    }

    public async Task UpdateCarStateAsync(string carId, CarState carState, CancellationToken ct = default)
    {
        using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        var carEntity = await dbContext.Cars.SingleOrDefaultAsync(ce => ce.CarId == carId, ct);
        if (carEntity == null)
        {
            throw new CrashException($"Car {carId} not found");
        }

        if (carState.DriveState.HasValue)
        {
            carEntity.DriveState = carState.DriveState.Value.ToString();
        }
        carEntity.Mileage = carState.Mileage ?? carEntity.Mileage;
        carEntity.LocationLat = carState.LocationLat ?? carEntity.LocationLat;
        carEntity.LocationLon = carState.LocationLon ?? carEntity.LocationLon;
        carEntity.RemainingFuel = carState.RemainingFuel ?? carEntity.RemainingFuel;
        carEntity.Alert = carState.Alert ?? carEntity.Alert;

        await dbContext.SaveChangesAsync();
        tran.Complete();
    }

    public async Task<Car> StartCarRent(string carId, string rentId, DateTime rentStartDate, CancellationToken ct = default)
    {
        using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        var carEntity = await dbContext.Cars.SingleOrDefaultAsync(ce => ce.CarId == carId, ct);
        if (carEntity == null)
        {
            throw new CrashException("Car not found");
        }

        if (await dbContext.CarRents.AnyAsync(cre => cre.RentId == rentId, ct))
        {
            throw new CrashException("Rent already exists");
        }

        if (await dbContext.CarRents.AnyAsync(cre => cre.CarId == carEntity.Id && cre.RentEndDate == null, ct))
        {
            throw new CrashException("Car already rented");
        }

        await dbContext.CarRents.AddAsync(
            new CarRentEntity
            {
                CarId = carEntity.Id,
                RentId = rentId,
                RentStartDate = rentStartDate,
                RentEndDate = null
            }, ct);

        await dbContext.SaveChangesAsync(ct);
        tran.Complete();

        return MapCar(carEntity);
    }

    public async Task FinishCarRent(string carId, string rentId, DateTime rentEndDate, CancellationToken ct = default)
    {
        using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        var carRentEntity = await dbContext.CarRents.SingleOrDefaultAsync(
            cre => cre.Car.CarId == carId && cre.RentId == rentId, ct);
        if (carRentEntity == null)
        {
            throw new CrashException("Rent not found");
        }

        if (carRentEntity.RentEndDate != null)
        {
            throw new CrashException("Rent already finished");
        }

        if (carRentEntity.RentStartDate > rentEndDate)
        {
            throw new CrashException($"Invalid {nameof(rentEndDate)}");
        }

        carRentEntity.RentEndDate = rentEndDate;
        await dbContext.SaveChangesAsync(ct);
        tran.Complete();
    }

    private static CarEntity MapCarEntity(Car c)
    {
        return new CarEntity
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
            DriveState = c.DriveState.ToString(),
            Mileage = c.Mileage,
            LocationLat = c.LocationLat,
            LocationLon = c.LocationLon,
            RemainingFuel = c.RemainingFuel,
            Alert = c.Alert,
            ApiKeyHash = c.ApiKeyHash,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate
        };
    }

    private static Car MapCar(CarEntity ce)
    {
        return new Car
        {
            CarId = ce.CarId,
            Brand = ce.Brand,
            Model = ce.Model,
            Color = ce.Color,
            ReleaseDate = ce.ReleaseDate,
            BodyStyle = Enum.Parse<CarBodyStyle>(ce.BodyStyle),
            DoorsCount = ce.DoorsCount,
            Transmission = Enum.Parse<CarTransmissionType>(ce.Transmission),
            FuelType = Enum.Parse<CarFuelType>(ce.FuelType),
            PricePerHour = ce.PricePerHour,
            PricePerKm = ce.PricePerKm,
            DriveState = Enum.Parse<CarDriveState>(ce.DriveState),
            Mileage = ce.Mileage,
            LocationLat = ce.LocationLat,
            LocationLon = ce.LocationLon,
            RemainingFuel = ce.RemainingFuel,
            Alert = ce.Alert,
            ApiKeyHash = ce.ApiKeyHash,
            CreatedDate = ce.CreatedDate,
            ModifiedDate = ce.ModifiedDate
        };
    }
}