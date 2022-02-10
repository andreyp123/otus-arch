using Common.Model.CarSvc;

namespace Common.Events.Messages;

public class CarStateMessage
{
    public string CarId { get; set; }
    public CarDriveState? DriveState { get; set; }
    public int? Mileage { get; set; }
}