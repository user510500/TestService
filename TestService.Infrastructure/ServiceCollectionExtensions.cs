using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TestService.Application.Interfaces;
using TestService.Domain.Common;
using TestService.Domain.Entities;
using TestService.Domain.Interfaces;
using TestService.Infrastructure.Cache;
using TestService.Infrastructure.Identity;
using TestService.Infrastructure.Messaging;
using TestService.Infrastructure.Options;
using TestService.Infrastructure.Persistence;

namespace TestService.Infrastructure;

/// <summary>
/// 基础设施层依赖注入扩展。
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置项
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        // MySQL + EF Core
        var mysqlConn = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("未配置 MySql 连接字符串（ConnectionStrings:MySql）");
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseMySql(mysqlConn, ServerVersion.AutoDetect(mysqlConn)));

        // 仓储：所有实体共用一个泛型实现
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Redis
        var redisConn = configuration.GetSection(RedisOptions.SectionName)["ConnectionString"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = ConfigurationOptions.Parse(redisConn);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        // RabbitMQ
        services.AddSingleton<RabbitMqMessageBus>();
        services.AddSingleton<IMessageBus>(sp => sp.GetRequiredService<RabbitMqMessageBus>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMqMessageBus>());

        // 测试消费者（开发用，验证消息端到端流转；生产应替换为真正的下游服务）
        services.AddHostedService<TestConsumerService>();

        // Identity
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
