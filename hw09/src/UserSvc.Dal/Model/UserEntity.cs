﻿namespace UserSvc.Dal.Model;

public class UserEntity
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
    public string? FullName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? DriverLicense { get; set; }
    public bool Verified { get; set; }
    public string PasswordHash { get; set; }
    public string Roles { get; set; }
}
