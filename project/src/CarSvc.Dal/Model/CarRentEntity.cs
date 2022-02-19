namespace CarSvc.Dal.Model;

public class CarRentEntity
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public string RentId { get; set; }
    public DateTime RentStartDate { get; set; }
    public DateTime? RentEndDate { get; set; }

    // Navigation properties
    public CarEntity Car { get; set; }
}