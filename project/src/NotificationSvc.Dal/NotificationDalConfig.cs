using Microsoft.Extensions.Configuration;
using NotificationSvc.Dal.Repositories;

namespace NotificationSvc.Dal
{
    public class NotificationDalConfig
    {
        private const string SECTION_NAME = "NotificationDal";
        
        public string? ConnectionString { get; set; }
        public bool AutoMigrate { get; set; }

        public NotificationDalConfig(IConfiguration configuration)
        {
            configuration.GetSection(SECTION_NAME).Bind(this);
        }
    }
}
