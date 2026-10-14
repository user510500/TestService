using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestService.Application.Interfaces;
using TestService.Domain.Entities;

namespace TestService.Infrastructure.Persistence;

/// <summary>
/// 数据库初始化器：开发环境自动建表 + 注入默认 admin 账号。
/// 生产环境建议使用 EF Core Migrations 替代。
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger<AppDbContext>>();
        var db = sp.GetRequiredService<AppDbContext>();
        var hasher = sp.GetRequiredService<IPasswordHasher>();

        try
        {
            // 开发环境：确保数据库与表已创建
            await db.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogInformation("数据库已就绪");

            // 默认 admin
            if (!await db.Users.AnyAsync(u => u.Username == "admin", cancellationToken))
            {
                var admin = new SysUser
                {
                    Username = "admin",
                    PasswordHash = hasher.Hash("admin123"),
                    Salt = string.Empty,
                    RealName = "系统管理员",
                    Email = "admin@example.com",
                    Status = 1
                };
                db.Users.Add(admin);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("已创建默认管理员 admin / admin123");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "数据库初始化失败，请检查连接串与数据库服务");
        }
    }
}
