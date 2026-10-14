using TestService.Application.Common;
using TestService.Application.Dtos;

namespace TestService.Application.Interfaces;

/// <summary>客户服务（CRUD）。</summary>
public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> QueryAsync(CustomerQuery query, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CustomerCreateRequest request, string currentUserName, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, CustomerUpdateRequest request, string currentUserName, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string currentUserName, CancellationToken cancellationToken = default);
}
