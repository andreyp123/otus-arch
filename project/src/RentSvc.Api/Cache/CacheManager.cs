using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RentSvc.Api.Cache;

public class CacheManager : ICacheManager
{
    private readonly ILogger<CacheManager> _logger;
    private readonly CacheManagerConfig _config;
    private readonly IDatabase _redisDb;
    
    public CacheManager(ILogger<CacheManager> logger, CacheManagerConfig config)
    {
        _logger = logger;
        _config = config;
        
        var options = ConfigurationOptions.Parse(config.RedisConnectionString);
        var connection = ConnectionMultiplexer.Connect(options);
        _redisDb = connection.GetDatabase();
    }
    
    public async Task<bool> SetIfNotExistAsync(string key, string value, CancellationToken ct = default)
    {
        try
        {
            return await _redisDb.StringSetAsync(
                new RedisKey(key),
                new RedisValue(value),
                TimeSpan.FromSeconds(_config.TtlSec),
                When.NotExists);
        }
        catch (Exception ex)
        {
            throw new CrashException($"Error accessing cache. {ex.Message}");
        }
    }
}