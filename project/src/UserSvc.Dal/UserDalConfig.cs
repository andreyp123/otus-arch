using Microsoft.Extensions.Configuration;

namespace UserSvc.Dal;

public class UserDalConfig
{
    private const string SECTION_NAME = "UserDal";

    public string ConnectionString { get; set; }
    public bool AutoMigrate { get; set; }

    public UserDalConfig(IConfiguration configuration)
    {
        configuration.GetSection(SECTION_NAME).Bind(this);
    }
}