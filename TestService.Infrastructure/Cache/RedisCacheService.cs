using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TestService.Domain.Interfaces;
using TestService.Infrastructure.Options;

namespace TestService.Infrastructure.Cache;

/// <summary>
/// Redis 缓存服务实现。
/// 使用 StackExchange.Redis。
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer multiplexer,
        IOptions<RedisOptions> options,
        ILogger<RedisCacheService> logger)
    {
        _multiplexer = multiplexer;
        _options = options.Value;
        _logger = logger;
    }

    private string Key(string key) => $"{_options.Prefix}{key}";

    public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _multiplexer.GetDatabase();
            var value = await db.StringGetAsync(Key(key));
            return value.HasValue ? value.ToString() : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET 失败：{Key}", key);
            return null;
        }
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _multiplexer.GetDatabase();
            await db.StringSetAsync(Key(key), value, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET 失败：{Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _multiplexer.GetDatabase();
            await db.KeyDeleteAsync(Key(key));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis DEL 失败：{Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _multiplexer.GetDatabase();
            return await db.KeyExistsAsync(Key(key));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis EXISTS 失败：{Key}", key);
            return false;
        }
    }
}
