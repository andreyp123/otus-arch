namespace Common.Model.RentSvc;

public class RentDto
{
    public string? RentId { get; set; }
    public string? Data { get; set; }
    public string? UserId { get; set; }
    public string? CarId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? State { get; set; }
    public string? Message { get; set; }
    public decimal Distance { get; set; } = 0;
    public decimal Amount { get; set; } = 0;
}
