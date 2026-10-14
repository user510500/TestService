using System.Text.Json;
using System.Text.Json.Serialization;
using TestService.Application;
using TestService.Application.Common;
using TestService.Infrastructure;
using TestService.Infrastructure.Persistence;
using TestService.Web.Filters;
using TestService.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// 1. 基础设施层（EF Core、Redis、RabbitMQ、JWT）
builder.Services.AddInfrastructure(builder.Configuration);

// 2. 应用层（业务服务）
builder.Services.AddApplication();

// 3. Web 层
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger 仅在开发环境启用
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 反向代理（Nginx）下获取真实客户端 IP
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
        | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// 全局鉴权中间件（白名单由 [AllowAnonymous] 标识）
app.UseMiddleware<AuthMiddleware>();

app.MapControllers();

// 开发环境：自动建表 + 注入 admin 账号
if (app.Environment.IsDevelopment())
{
    await DbInitializer.SeedAsync(app.Services);
}

app.Run();
