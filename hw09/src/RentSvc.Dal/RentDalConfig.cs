using Microsoft.Extensions.Configuration;

namespace RentSvc.Dal
{
    public class RentDalConfig
    {
        private const string SECTION_NAME = "RentDal";

        public string ConnectionString { get; set; }
        public bool AutoMigrate { get; set; }

        public RentDalConfig(IConfiguration configuration)
        {
            configuration.GetSection(SECTION_NAME).Bind(this);
        }
    }
}
