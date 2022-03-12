using System;
using Common.Model.RentSvc;

namespace RentSvc.Api.Helpers;

public static class RentMapper
{
    public static RentDto MapRentDto(Rent rent, bool full = false)
    {
        var rentDto = new RentDto
        {
            RentId = rent.RentId,
            UserId = rent.UserId,
            CarId = rent.CarId,
            Data = rent.Data,
            CreatedDate = rent.CreatedDate,
            StartDate = rent.StartDate,
            EndDate = rent.EndDate,
            State = rent.State.ToString(),
            Message = rent.Message
        };

        if (full)
        {
            DateTime now = DateTime.UtcNow;
            rentDto.Distance = RentCalculator.CalcDistance(rent);
            rentDto.Amount = RentCalculator.CalcAmount(rent, now);
        }

        return rentDto;
    }
}