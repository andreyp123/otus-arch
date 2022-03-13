using Common.Model.UserSvc;

namespace UserSvc.Api.Helpers;

public static class UserMapper
{
    public static UserDto MapUserDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DriverLicense = user.DriverLicense,
            Verified = user.Verified,
            Roles = user.Roles,
            CreatedDate = user.CreatedDate,
            ModifiedDate = user.ModifiedDate
        };
    }
}