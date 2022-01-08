using Microsoft.Extensions.Configuration;

namespace OrderSvc.Api.BillingClient;

public class BillingClientConfig
{
    public string Url { get; set; }

    public BillingClientConfig(IConfiguration configuration)
    {
        configuration.GetSection(nameof(BillingClient)).Bind(this);
    }
}