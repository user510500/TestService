using TestService.Application.Common;
using TestService.Application.Dtos;

namespace TestService.Application.Interfaces;

/// <summary>销售订单服务。</summary>
public interface ISalesOrderService
{
    Task<PagedResult<SalesOrderDto>> QueryAsync(SalesOrderQuery query, CancellationToken cancellationToken = default);
    Task<SalesOrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(SalesOrderCreateRequest request, string currentUserName, CancellationToken cancellationToken = default);
    Task ConfirmAsync(Guid id, string currentUserName, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid id, string currentUserName, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string currentUserName, CancellationToken cancellationToken = default);
}
