using Microsoft.Extensions.Configuration;

namespace BillingSvc.Repository
{
    public class AccountRepositoryConfig
    {
        public string? ConnectionString { get; set; }

        public AccountRepositoryConfig(IConfiguration configuration)
        {
            configuration.GetSection(nameof(AccountRepository)).Bind(this);
        }
    }
}
