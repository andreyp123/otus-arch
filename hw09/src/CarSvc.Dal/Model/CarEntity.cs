namespace CarSvc.Dal.Model;

public class CarEntity
{
    // Identity
    public int Id { get; set; }
    public string CarId { get; set; }

    // Details
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string BodyStyle { get; set; }
    public int DoorsCount { get; set; }
    public string Transmission { get; set; }
    public string FuelType { get; set; }

    // Tariff
    public decimal PricePerKm { get; set; }
    public decimal PricePerHour { get; set; }

    // State
    public int Mileage { get; set; }
    public decimal LocationLat { get; set; }
    public decimal LocationLon { get; set; }
    public decimal RemainingFuel { get; set; }
    public string? Alert { get; set; }

    // Technical fields
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    // Navigation properties
    public ICollection<CarRentEntity> CarRents { get; set; }
}
