namespace Common.Model.RentSvc;

public class User
{
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? DriverLicense { get; set; }
    public bool Verified { get; set; }
    public DateTime ModifiedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
}
