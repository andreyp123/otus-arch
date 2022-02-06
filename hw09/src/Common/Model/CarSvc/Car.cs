namespace Common.Model.CarSvc;

public class Car
{
    public string? CarId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public DateTime ReleaseDate { get; set; }
    public CarBodyStyles BodyStyle { get; set; }
    public int DoorsCount { get; set; }
    public CarTransmissionTypes Transmission { get; set; }
    public CarFuelTypes FuelType { get; set; }
        
    public decimal PricePerKm { get; set; }
    public decimal PricePerHour { get; set; }
        
    public int Mileage { get; set; }
    public decimal LocationLat { get; set; }
    public decimal LocationLon { get; set; }
    public decimal RemainingFuel { get; set; }
    public string? Alert { get; set; }
        
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    
    public bool IsRent { get; set; }
}