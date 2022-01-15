using Microsoft.Extensions.Configuration;

namespace OrderSvc.Repository
{
    public class OrderRepositoryConfig
    {
        public string ConnectionString { get; set; }
        public bool AutoMigrate { get; set; }

        public OrderRepositoryConfig(IConfiguration configuration)
        {
            configuration.GetSection(nameof(OrderRepository)).Bind(this);
        }
    }
}
