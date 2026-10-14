namespace TestService.Application.Dtos;

/// <summary>登录请求</summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>注册请求</summary>
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

/// <summary>登录响应：返回 access token 与当前用户基本信息</summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public UserInfoDto User { get; set; } = new();
}

/// <summary>当前用户信息（供前端展示）</summary>
public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public List<string> Roles { get; set; } = new();
}
