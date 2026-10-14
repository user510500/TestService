namespace TestService.Infrastructure.Options;

/// <summary>JWT 配置</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "TestService";
    public string Audience { get; set; } = "TestService.Client";
    /// <summary>对称加密密钥（生产环境建议 32 位以上）</summary>
    public string Secret { get; set; } = "test-service-default-secret-key-please-change";
    public int ExpiresMinutes { get; set; } = 480; // 8 小时
}
