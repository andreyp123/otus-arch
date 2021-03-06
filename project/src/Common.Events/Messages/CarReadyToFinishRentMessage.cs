using Common.Model.CarSvc;

namespace Common.Events.Messages;

public class CarReadyToFinishRentMessage : MessageBase
{
    public string RentId { get; set; }
    public string UserId { get; set; }
    public CarDto Car { get; set; }
}