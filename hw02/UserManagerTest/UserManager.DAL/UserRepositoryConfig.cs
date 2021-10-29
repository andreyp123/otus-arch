using Microsoft.Extensions.Configuration;

namespace UserManager.DAL
{
    public class UserRepositoryConfig
    {
        public string ConnectionString { get; set; }

        public UserRepositoryConfig(IConfiguration configuration)
        {
            configuration.GetSection(nameof(UserRepository)).Bind(this);
        }
    }
}
