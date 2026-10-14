namespace TestService.Application.Interfaces;

/// <summary>
/// JWT 令牌服务：负责签发与解析 JWT。
/// 由 Infrastructure 层实现。
/// </summary>
public interface IJwtTokenService
{
    /// <summary>签发访问令牌</summary>
    string GenerateToken(Guid userId, string username, IEnumerable<string> roles, IEnumerable<string>? permissions = null);

    /// <summary>解析访问令牌</summary>
    JwtPayload? ParseToken(string token);
}

public class JwtPayload
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
}
