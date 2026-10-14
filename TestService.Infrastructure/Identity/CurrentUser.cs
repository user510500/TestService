using TestService.Application.Interfaces;

namespace TestService.Infrastructure.Identity;

/// <summary>
/// 当前用户上下文（由 Web 层的鉴权中间件从 JWT 解析后填充）。
/// 使用 AsyncLocal 跨请求传递，避免每个 Service 显式传 userId。
/// </summary>
public class CurrentUser : ICurrentUser
{
    private static readonly AsyncLocal<JwtPayload?> _current = new();

    public Guid? UserId => _current.Value?.UserId;
    public string? Username => _current.Value?.Username;
    public bool IsAuthenticated => _current.Value is not null;

    public static void Set(JwtPayload? payload) => _current.Value = payload;

    public static void Clear() => _current.Value = null;
}
