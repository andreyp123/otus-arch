using Microsoft.Extensions.Configuration;

namespace RentSvc.Api.Cache;

public class CacheManagerConfig
{
    private const string SECTION_NAME = "CacheManager";

    public string RedisConnectionString { get; set; }
    public int TtlSec { get; set; }

    public CacheManagerConfig(IConfiguration configuration)
    {
        configuration.GetSection(SECTION_NAME).Bind(this);
    }
}