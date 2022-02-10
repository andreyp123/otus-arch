using System;

namespace RentSvc.Dal.Model;

public class RentEntity
{
    public int Id { get; set; }
    public string RentId { get; set; }
    public string Data { get; set; }
    public string UserId { get; set; }
    public string CarId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string State { get; set; }
    public string? Message { get; set; }
    public int? StartMileage { get; set; }
    public int? Mileage { get; set; }
    public decimal? PricePerKm { get; set; }
    public decimal? PricePerHour { get; set; }
}
