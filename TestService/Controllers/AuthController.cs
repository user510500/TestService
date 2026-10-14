using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestService.Application.Common;
using TestService.Application.Dtos;
using TestService.Application.Interfaces;

namespace TestService.Web.Controllers;

/// <summary>
/// 认证 Controller：登录、注册、获取当前用户。
/// 登录/注册 加入白名单，无需 Token。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;

    public AuthController(IAuthService authService, ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>用户登录</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return ApiResponse<LoginResponse>.Ok(result);
    }

    /// <summary>用户注册</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ApiResponse> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        await _authService.RegisterAsync(request, cancellationToken);
        return ApiResponse.Ok("注册成功");
    }

    /// <summary>获取当前登录用户信息</summary>
    [HttpGet("me")]
    public async Task<ApiResponse<UserInfoDto>> Me(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return ApiResponse<UserInfoDto>.Fail("未登录", 401);
        var user = await _authService.GetCurrentUserAsync(_currentUser.UserId.Value, cancellationToken);
        return ApiResponse<UserInfoDto>.Ok(user);
    }
}
