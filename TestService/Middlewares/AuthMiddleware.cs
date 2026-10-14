using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using TestService.Application.Interfaces;
using TestService.Infrastructure.Identity;

namespace TestService.Web.Middlewares;

/// <summary>
/// JWT 鉴权中间件：
/// 1. 解析请求头中的 Bearer Token
/// 2. 通过 JWT 服务校验并解析出 JwtPayload
/// 3. 写入到 CurrentUser（基于 AsyncLocal 跨 Service 传递）
/// 4. 白名单（AllowAnonymous 或标记 [AllowAnonymous]）直接放行
/// 5. 其余接口必须登录，否则 401
/// </summary>
public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthMiddleware> _logger;

    public AuthMiddleware(RequestDelegate next, IJwtTokenService jwtTokenService, ILogger<AuthMiddleware> logger)
    {
        _next = next;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        CurrentUser.Clear();
        try
        {
            var endpoint = context.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() is not null;

            // 控制器或 Action 上有 [AllowAnonymous] 的放行
            if (allowAnonymous)
            {
                await _next(context);
                return;
            }

            // 没带 Token 直接 401
            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader)
                || !authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await RejectAsync(context, "缺少访问令牌");
                return;
            }

            var token = authHeader.ToString()["Bearer ".Length..].Trim();
            var payload = _jwtTokenService.ParseToken(token);
            if (payload is null)
            {
                await RejectAsync(context, "令牌无效或已过期");
                return;
            }

            CurrentUser.Set(payload);
            await _next(context);
        }
        finally
        {
            CurrentUser.Clear();
        }
    }

    private static async Task RejectAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json; charset=utf-8";
        var json = System.Text.Json.JsonSerializer.Serialize(
            new { code = 401, message, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
        await context.Response.WriteAsync(json);
    }
}
