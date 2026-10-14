using TestService.Application.Dtos;

namespace TestService.Application.Interfaces;

/// <summary>
/// 认证服务：负责登录、注册、Token 签发。
/// </summary>
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<UserInfoDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
