namespace Common.Model.CarSvc;

public class CreateCarDto
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? BodyStyle { get; set; }
    public int DoorsCount { get; set; }
    public string? Transmission { get; set; }
    public string? FuelType { get; set; }

    public decimal PricePerKm { get; set; }
    public decimal PricePerHour { get; set; }
}