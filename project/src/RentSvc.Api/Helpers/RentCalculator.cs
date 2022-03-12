using System;
using Common.Model.RentSvc;

namespace RentSvc.Api.Helpers;

public static class RentCalculator
{
    public static decimal? CalcDistance(Rent rent)
    {
        return rent.StartMileage != null && rent.Mileage != null
            ? rent.Mileage - rent.StartMileage
            : null;
    }

    public static decimal? CalcAmount(Rent rent, DateTime now)
    {
        if (rent.PricePerKm == null || rent.PricePerHour == null)
            return null;
        
        var distance = CalcDistance(rent);
        if (!distance.HasValue)
            return null;
        
        if (!rent.StartDate.HasValue)
            return null;

        var hours = (decimal)((rent.EndDate ?? now) - rent.StartDate.Value).TotalHours;

        return Math.Round((decimal)(rent.PricePerHour * hours + rent.PricePerKm * distance), 2);
    }
}