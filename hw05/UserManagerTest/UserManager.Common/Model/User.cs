namespace UserManager.Common.Model
{
    public class User
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string[] Roles { get; set; }
    }
}
