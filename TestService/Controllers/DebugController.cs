using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using TestService.Application.Common;
using TestService.Application.Interfaces;
using TestService.Domain.Interfaces;

namespace TestService.Web.Controllers;

/// <summary>
/// 调试用 Controller：验证 Redis / RabbitMQ 连通性。
/// 仅在开发环境使用，生产环境应删除或限制访问。
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class DebugController : ControllerBase
{
    private readonly ICacheService _cache;
    private readonly IConnectionMultiplexer _redis;

    public DebugController(ICacheService cache, IConnectionMultiplexer redis)
    {
        _cache = cache;
        _redis = redis;
    }

    /// <summary>写一个 key 到 Redis 并立刻读回来。</summary>
    [HttpGet("redis/set")]
    public async Task<ApiResponse<object>> SetRedis([FromQuery] string key, [FromQuery] string value)
    {
        await _cache.SetStringAsync(key, value, TimeSpan.FromMinutes(5));
        var readBack = await _cache.GetStringAsync(key);
        return ApiResponse<object>.Ok(new
        {
            key,
            written = value,
            readBack,
            match = readBack == value,
            redisConnected = _redis.IsConnected
        });
    }

    /// <summary>读一个 key 的值。</summary>
    [HttpGet("redis/get")]
    public async Task<ApiResponse<object>> GetRedis([FromQuery] string key)
    {
        var value = await _cache.GetStringAsync(key);
        return ApiResponse<object>.Ok(new
        {
            key,
            value,
            redisConnected = _redis.IsConnected
        });
    }

    /// <summary>查看 Redis 连接信息（endpoints、版本等）。</summary>
    [HttpGet("redis/info")]
    public ApiResponse<object> RedisInfo()
    {
        var endpoints = _redis.GetEndPoints();
        var info = endpoints.Select(ep =>
        {
            var server = _redis.GetServer(ep);
            return new
            {
                endpoint = ep.ToString(),
                isConnected = server.IsConnected,
                role = server.IsReplica ? "replica" : "master",
                version = server.Version?.ToString() ?? "unknown"
            };
        });
        return ApiResponse<object>.Ok(new
        {
            isConnected = _redis.IsConnected,
            endpoints = info
        });
    }
}
