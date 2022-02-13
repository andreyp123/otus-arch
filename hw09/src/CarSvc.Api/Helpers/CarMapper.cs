using System;
using Common.Model.CarSvc;

namespace CarSvc.Api.Helpers;

public static class CarMapper
{
    public static Car MapCar(CreateCarDto cc)
    {
        return new Car
        {
            Brand = cc.Brand,
            Model = cc.Model,
            Color = cc.Color,
            ReleaseDate = cc.ReleaseDate,
            BodyStyle = Enum.Parse<CarBodyStyle>(cc.BodyStyle),
            DoorsCount = cc.DoorsCount,
            Transmission = Enum.Parse<CarTransmissionType>(cc.Transmission),
            FuelType = Enum.Parse<CarFuelType>(cc.FuelType),
            PricePerHour = cc.PricePerHour,
            PricePerKm = cc.PricePerKm
        };
    }
    
    public static CarDto MapCarDto(Car c)
    {
        return new CarDto
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
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate
        };
    }
}