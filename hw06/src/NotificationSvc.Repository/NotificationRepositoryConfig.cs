using Microsoft.Extensions.Configuration;

namespace NotificationSvc.Repository
{
    public class NotificationRepositoryConfig
    {
        public string? ConnectionString { get; set; }
        public bool AutoMigrate { get; set; }

        public NotificationRepositoryConfig(IConfiguration configuration)
        {
            configuration.GetSection(nameof(NotificationRepository)).Bind(this);
        }
    }
}
