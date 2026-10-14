using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using TestService.Application.Common;
using TestService.Domain.Exceptions;

namespace TestService.Web.Filters;

/// <summary>
/// 全局异常过滤器：统一捕获异常并返回 ApiResponse。
/// 按部门规范：Controller 不写 try/catch，由该过滤器统一处理。
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;
        switch (ex)
        {
            case DomainException domainEx:
                _logger.LogWarning("业务异常：{Message}", domainEx.Message);
                context.Result = new ObjectResult(ApiResponse<object>.Fail(domainEx.Message, domainEx.Code))
                {
                    StatusCode = 200 // 业务异常仍以 200 返回，前端按 Code 判断
                };
                context.ExceptionHandled = true;
                break;

            case UnauthorizedAccessException:
                context.Result = new ObjectResult(ApiResponse<object>.Fail("未授权", 401))
                {
                    StatusCode = 200
                };
                context.ExceptionHandled = true;
                break;

            default:
                _logger.LogError(ex, "未处理异常：{Message}", ex.Message);
                context.Result = new ObjectResult(ApiResponse<object>.Fail("服务器内部错误", 500))
                {
                    StatusCode = 500
                };
                context.ExceptionHandled = true;
                break;
        }
    }
}
