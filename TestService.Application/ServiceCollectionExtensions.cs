using Microsoft.Extensions.DependencyInjection;
using TestService.Application.Interfaces;
using TestService.Application.Services;

namespace TestService.Application;

/// <summary>
/// 应用层依赖注入扩展。
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        return services;
    }
}
