using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using TestService.Application.Common;
using TestService.Application.Dtos;
using TestService.Application.Interfaces;
using TestService.Domain.Entities;
using TestService.Domain.Exceptions;
using TestService.Domain.Interfaces;

namespace TestService.Application.Services;

/// <summary>商品服务实现。</summary>
public class ProductService : IProductService
{
    private readonly IRepository<ProProduct> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IRepository<ProProduct> repo, IUnitOfWork uow, ILogger<ProductService> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> QueryAsync(ProductQuery query, CancellationToken cancellationToken = default)
    {
        Expression<Func<ProProduct, bool>> predicate = p => !p.IsDeleted;
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            predicate = p => !p.IsDeleted && (p.Code.Contains(query.Keyword) || p.Name.Contains(query.Keyword));
        if (!string.IsNullOrWhiteSpace(query.Category))
            predicate = predicate.And(p => p.Category == query.Category);
        if (query.Status.HasValue)
            predicate = predicate.And(p => p.Status == query.Status.Value);

        var total = await _repo.CountAsync(predicate, cancellationToken);
        var items = (await _repo.GetListAsync(predicate, cancellationToken))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(Map)
            .ToList();

        return new PagedResult<ProductDto>
        {
            Items = items, Total = total, PageIndex = query.PageIndex, PageSize = query.PageSize
        };
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var p = await _repo.GetByIdAsync(id, cancellationToken);
        return p is null || p.IsDeleted ? null : Map(p);
    }

    public async Task<Guid> CreateAsync(ProductCreateRequest request, string currentUserName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("编码和名称不能为空");
        if (await _repo.ExistsAsync(p => p.Code == request.Code, cancellationToken))
            throw new DomainException("商品编码已存在");

        var entity = new ProProduct
        {
            Code = request.Code,
            Name = request.Name,
            Category = request.Category,
            Unit = request.Unit,
            Price = request.Price,
            CostPrice = request.CostPrice,
            StockQty = request.StockQty,
            Description = request.Description,
            Status = 1,
            CreatedBy = currentUserName
        };
        await _repo.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("创建商品 {Code}", entity.Code);
        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, ProductUpdateRequest request, string currentUserName, CancellationToken cancellationToken = default)
    {
        var p = await _repo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("商品不存在", 404);
        if (p.IsDeleted) throw new DomainException("商品已删除", 404);

        p.Name = request.Name;
        p.Category = request.Category;
        p.Unit = request.Unit;
        p.Price = request.Price;
        p.CostPrice = request.CostPrice;
        p.StockQty = request.StockQty;
        p.Status = request.Status;
        p.Description = request.Description;
        p.UpdatedAt = DateTime.UtcNow;
        p.UpdatedBy = currentUserName;
        await _repo.UpdateAsync(p, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, string currentUserName, CancellationToken cancellationToken = default)
    {
        var p = await _repo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("商品不存在", 404);
        p.IsDeleted = true;
        p.UpdatedAt = DateTime.UtcNow;
        p.UpdatedBy = currentUserName;
        await _repo.UpdateAsync(p, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    private static ProductDto Map(ProProduct p) => new()
    {
        Id = p.Id, Code = p.Code, Name = p.Name, Category = p.Category, Unit = p.Unit,
        Price = p.Price, CostPrice = p.CostPrice, StockQty = p.StockQty,
        Status = p.Status, Description = p.Description, CreatedAt = p.CreatedAt
    };
}
