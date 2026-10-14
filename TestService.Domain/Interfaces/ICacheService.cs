namespace TestService.Domain.Interfaces;

/// <summary>
/// 缓存服务接口（领域层）。
/// 由 Infrastructure 层的 Redis 实现。
/// </summary>
public interface ICacheService
{
    Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);
    Task SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
