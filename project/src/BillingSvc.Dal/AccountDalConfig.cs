using BillingSvc.Dal.Repositories;
using Microsoft.Extensions.Configuration;

namespace BillingSvc.Dal
{
    public class AccountDalConfig
    {
        private const string SECTION_NAME = "AccountDal";

        public string? ConnectionString { get; set; }
        public bool AutoMigrate { get; set; }

        public AccountDalConfig(IConfiguration configuration)
        {
            configuration.GetSection(SECTION_NAME).Bind(this);
        }
    }
}
