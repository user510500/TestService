namespace TestService.Application.Interfaces;

/// <summary>
/// 当前登录用户上下文（由 Web 层从 Token 解析后填充）。
/// Application 服务通过它获取当前操作人。
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
}
