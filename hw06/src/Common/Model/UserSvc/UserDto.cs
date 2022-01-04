namespace Common.Model.UserSvc
{
    public class UserDto
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string[]? Roles { get; set; }
    }
}
