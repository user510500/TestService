namespace TestService.Infrastructure.Options;

/// <summary>Redis 配置</summary>
public class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>连接字符串，如：server1:6379,server2:6379,password=xxx</summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>全局 key 前缀</summary>
    public string Prefix { get; set; } = "TestService:";
}
