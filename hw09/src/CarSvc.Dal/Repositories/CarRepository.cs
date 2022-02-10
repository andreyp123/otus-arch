using System.Transactions;
using Common;
using Common.Model.CarSvc;
using Microsoft.EntityFrameworkCore;
using CarSvc.Dal.Model;

namespace CarSvc.Dal.Repositories
{
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

        public async Task<(Car[], int)> GetCarsAsync(int start, int size, CancellationToken ct = default)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

            var query = dbContext.Cars;
            var total = await query
                .CountAsync(ct);
            var cars = await query
                .OrderByDescending(ne => ne.CreatedDate)
                .Skip(start)
                .Take(size)
                .Select(ce => MapCar(ce))
                .ToArrayAsync(ct);
            return (cars, total);
        }

        public async Task StartCarRent(string carId, string rentId, DateTime rentStartDate, CancellationToken ct = default)
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
            if (await dbContext.CarRents.AnyAsync(cre => cre.CarId == carEntity.Id && cre.RentEndDate != null, ct))
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
                Mileage = c.Mileage,
                LocationLat = c.LocationLat,
                LocationLon = c.LocationLon,
                RemainingFuel = c.RemainingFuel,
                Alert = c.Alert,
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
                Mileage = ce.Mileage,
                LocationLat = ce.LocationLat,
                LocationLon = ce.LocationLon,
                RemainingFuel = ce.RemainingFuel,
                Alert = ce.Alert,
                CreatedDate = ce.CreatedDate,
                ModifiedDate = ce.ModifiedDate
            };
        }
    }
}
