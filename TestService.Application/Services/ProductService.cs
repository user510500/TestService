using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestService.Application.Common;
using TestService.Application.Dtos;
using TestService.Application.Interfaces;
using TestService.Domain.Entities;
using TestService.Domain.Exceptions;
using TestService.Domain.Interfaces;

namespace TestService.Application.Services;

/// <summary>
/// 商品服务实现（带 Redis 缓存）。
///
/// 缓存策略：Cache-Aside 模式
/// - 读（GetById / Query）：先查 Redis，命中返回；未命中查 MySQL，再写回 Redis
/// - 写（Create / Update / Delete）：先改 MySQL，再删 Redis 缓存
///   （不写 Redis，因为"删除"操作没有数据可写；下次读时自动重新填充）
/// </summary>
public class ProductService : IProductService
{
    private readonly IRepository<ProProduct> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly ILogger<ProductService> _logger;

    // 详情缓存 10 分钟（商品数据变化频率不高）
    private static readonly TimeSpan DetailTtl = TimeSpan.FromMinutes(10);
    // 列表缓存 2 分钟（分页数据变化更频繁，且更占空间）
    private static readonly TimeSpan ListTtl = TimeSpan.FromMinutes(2);

    public ProductService(
        IRepository<ProProduct> repo,
        IUnitOfWork uow,
        ICacheService cache,
        ILogger<ProductService> logger)
    {
        _repo = repo;
        _uow = uow;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> QueryAsync(ProductQuery query, CancellationToken cancellationToken = default)
    {
        // 列表缓存策略：仅缓存"第一页 + 无任何筛选"的常见场景
        // 因为带 keyword / category / status 的查询组合爆炸，缓存命中率低
        var isSimpleQuery = query.PageIndex == 1
                            && string.IsNullOrWhiteSpace(query.Keyword)
                            && string.IsNullOrWhiteSpace(query.Category)
                            && !query.Status.HasValue;

        if (isSimpleQuery)
        {
            var cacheKey = CacheKeys.ProductList(query.PageIndex, query.PageSize, null, null, null);
            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (cached is not null)
            {
                _logger.LogInformation("商品列表缓存命中 {Key}", cacheKey);
                return JsonSerializer.Deserialize<PagedResult<ProductDto>>(cached)!;
            }

            var result = await QueryFromDbAsync(query, cancellationToken);
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), ListTtl, cancellationToken);
            _logger.LogInformation("商品列表已写入缓存 {Key}", cacheKey);
            return result;
        }

        // 复杂查询：直接走数据库（不缓存）
        return await QueryFromDbAsync(query, cancellationToken);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.ProductDetail(id);

        // 1. 查缓存
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation("商品 {Id} 缓存命中", id);
            return JsonSerializer.Deserialize<ProductDto>(cached);
        }

        // 2. 缓存未命中，查数据库
        var p = await _repo.GetByIdAsync(id, cancellationToken);
        if (p is null || p.IsDeleted)
        {
            // 商品不存在：不缓存（避免恶意探测缓存）
            return null;
        }
        var dto = Map(p);

        // 3. 写回缓存
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), DetailTtl, cancellationToken);
        _logger.LogInformation("商品 {Id} 已写入缓存", id);

        return dto;
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

        // 失效"商品列表第一页"缓存（新增商品后第一页会有变化）
        await InvalidateListCacheAsync(cancellationToken);

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
        _logger.LogInformation("更新商品 {Id}", id);

        // 失效缓存
        await _cache.RemoveAsync(CacheKeys.ProductDetail(id), cancellationToken);
        await InvalidateListCacheAsync(cancellationToken);
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
        _logger.LogInformation("删除商品 {Id}", id);

        // 失效缓存
        await _cache.RemoveAsync(CacheKeys.ProductDetail(id), cancellationToken);
        await InvalidateListCacheAsync(cancellationToken);
    }

    // 私有方法：实际查库逻辑（缓存逻辑外提，便于复用）
    private async Task<PagedResult<ProductDto>> QueryFromDbAsync(ProductQuery query, CancellationToken cancellationToken)
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

    // 失效列表缓存的私有方法
    // 当前实现：只失效"无筛选条件"的那一个 key
    // 进阶实现（下面注释里讲）：用 SCAN 匹配 product:list:* 全部失效
    private async Task InvalidateListCacheAsync(CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(CacheKeys.ProductList(1, 20, null, null, null), cancellationToken);
    }

    private static ProductDto Map(ProProduct p) => new()
    {
        Id = p.Id, Code = p.Code, Name = p.Name, Category = p.Category, Unit = p.Unit,
        Price = p.Price, CostPrice = p.CostPrice, StockQty = p.StockQty,
        Status = p.Status, Description = p.Description, CreatedAt = p.CreatedAt
    };
}
