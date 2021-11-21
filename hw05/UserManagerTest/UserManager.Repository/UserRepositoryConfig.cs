using Microsoft.Extensions.Configuration;

namespace UserManager.Repository
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
