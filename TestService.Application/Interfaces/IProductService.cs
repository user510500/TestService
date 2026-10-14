using TestService.Application.Common;
using TestService.Application.Dtos;

namespace TestService.Application.Interfaces;

/// <summary>商品服务。</summary>
public interface IProductService
{
    Task<PagedResult<ProductDto>> QueryAsync(ProductQuery query, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(ProductCreateRequest request, string currentUserName, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, ProductUpdateRequest request, string currentUserName, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string currentUserName, CancellationToken cancellationToken = default);
}
