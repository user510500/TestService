using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestService.Application.Common;

namespace TestService.Web.Controllers;

/// <summary>健康检查 Controller（白名单）。</summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ApiResponse<object> Get() => ApiResponse<object>.Ok(new
    {
        status = "ok",
        server = Environment.MachineName,
        time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
    });
}
