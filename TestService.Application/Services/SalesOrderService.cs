using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using TestService.Application.Common;
using TestService.Application.Dtos;
using TestService.Application.Interfaces;
using TestService.Domain.Entities;
using TestService.Domain.Exceptions;
using TestService.Domain.Interfaces;

namespace TestService.Application.Services;

/// <summary>销售订单服务实现。</summary>
public class SalesOrderService : ISalesOrderService
{
    private readonly IRepository<SoSalesOrder> _orderRepo;
    private readonly IRepository<SoSalesOrderItem> _itemRepo;
    private readonly IRepository<CusCustomer> _customerRepo;
    private readonly IRepository<ProProduct> _productRepo;
    private readonly IUnitOfWork _uow;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<SalesOrderService> _logger;

    public SalesOrderService(
        IRepository<SoSalesOrder> orderRepo,
        IRepository<SoSalesOrderItem> itemRepo,
        IRepository<CusCustomer> customerRepo,
        IRepository<ProProduct> productRepo,
        IUnitOfWork uow,
        IMessageBus messageBus,
        ILogger<SalesOrderService> logger)
    {
        _orderRepo = orderRepo;
        _itemRepo = itemRepo;
        _customerRepo = customerRepo;
        _productRepo = productRepo;
        _uow = uow;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<PagedResult<SalesOrderDto>> QueryAsync(SalesOrderQuery query, CancellationToken cancellationToken = default)
    {
        Expression<Func<SoSalesOrder, bool>> predicate = o => !o.IsDeleted;
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            predicate = predicate.And(o => o.OrderNo.Contains(query.Keyword));
        if (query.Status.HasValue)
            predicate = predicate.And(o => o.Status == query.Status.Value);
        if (query.CustomerId.HasValue)
            predicate = predicate.And(o => o.CustomerId == query.CustomerId.Value);

        var total = await _orderRepo.CountAsync(predicate, cancellationToken);
        var orders = (await _orderRepo.GetListAsync(predicate, cancellationToken))
            .OrderByDescending(o => o.CreatedAt)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
        var customers = customerIds.Count == 0
            ? new List<CusCustomer>()
            : await _customerRepo.GetListAsync(c => customerIds.Contains(c.Id), cancellationToken);

        var items = orders.Select(o => new SalesOrderDto
        {
            Id = o.Id,
            OrderNo = o.OrderNo,
            CustomerId = o.CustomerId,
            CustomerName = customers.FirstOrDefault(c => c.Id == o.CustomerId)?.Name,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            DiscountAmount = o.DiscountAmount,
            FinalAmount = o.FinalAmount,
            Status = o.Status,
            Remark = o.Remark,
            CreatedAt = o.CreatedAt
        }).ToList();

        return new PagedResult<SalesOrderDto>
        {
            Items = items, Total = total, PageIndex = query.PageIndex, PageSize = query.PageSize
        };
    }

    public async Task<SalesOrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken);
        if (order is null || order.IsDeleted) return null;

        var customer = await _customerRepo.GetByIdAsync(order.CustomerId, cancellationToken);
        var items = await _itemRepo.GetListAsync(i => i.OrderId == order.Id, cancellationToken);
        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = productIds.Count == 0
            ? new List<ProProduct>()
            : await _productRepo.GetListAsync(p => productIds.Contains(p.Id), cancellationToken);

        return new SalesOrderDto
        {
            Id = order.Id,
            OrderNo = order.OrderNo,
            CustomerId = order.CustomerId,
            CustomerName = customer?.Name,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            Status = order.Status,
            Remark = order.Remark,
            CreatedAt = order.CreatedAt,
            Items = items.Select(i => new SalesOrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = products.FirstOrDefault(p => p.Id == i.ProductId)?.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SubTotal = i.SubTotal
            }).ToList()
        };
    }

    public async Task<Guid> CreateAsync(SalesOrderCreateRequest request, string currentUserName, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
            throw new DomainException("订单至少包含一个商品");

        var customer = await _customerRepo.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new DomainException("客户不存在", 404);

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _productRepo.GetListAsync(p => productIds.Contains(p.Id), cancellationToken);
        var productMap = products.ToDictionary(p => p.Id);

        decimal total = 0;
        var lines = new List<SoSalesOrderItem>();
        foreach (var item in request.Items)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
                throw new DomainException($"商品 {item.ProductId} 不存在");
            if (item.Quantity <= 0)
                throw new DomainException("数量必须大于 0");
            if (product.StockQty < item.Quantity)
                throw new DomainException($"商品 {product.Name} 库存不足");

            var price = item.UnitPrice ?? product.Price;
            var subTotal = price * item.Quantity;
            total += subTotal;
            lines.Add(new SoSalesOrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = price,
                SubTotal = subTotal
            });
        }

        var order = new SoSalesOrder
        {
            OrderNo = $"SO{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            TotalAmount = total,
            DiscountAmount = request.DiscountAmount,
            FinalAmount = total - request.DiscountAmount,
            Status = 1, // 草稿
            Remark = request.Remark,
            CreatedBy = currentUserName,
            Items = lines
        };
        await _orderRepo.AddAsync(order, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // 异步通知下游（如：库存服务、消息推送）
        await _messageBus.PublishAsync(
            "erp.events",
            "sales.order.created",
            new { OrderId = order.Id, OrderNo = order.OrderNo, Total = total },
            cancellationToken);

        _logger.LogInformation("创建销售订单 {OrderNo}，金额 {Amount}", order.OrderNo, order.FinalAmount);
        return order.Id;
    }

    public async Task ConfirmAsync(Guid id, string currentUserName, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("订单不存在", 404);
        if (order.Status != 1)
            throw new DomainException("仅草稿状态的订单可以确认");

        // 扣减库存
        var items = await _itemRepo.GetListAsync(i => i.OrderId == order.Id, cancellationToken);
        foreach (var item in items)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null) continue;
            product.StockQty -= item.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = currentUserName;
            await _productRepo.UpdateAsync(product, cancellationToken);
        }

        order.Status = 2;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = currentUserName;
        await _orderRepo.UpdateAsync(order, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _messageBus.PublishAsync(
            "erp.events",
            "sales.order.confirmed",
            new { OrderId = order.Id, OrderNo = order.OrderNo },
            cancellationToken);
    }

    public async Task CancelAsync(Guid id, string currentUserName, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("订单不存在", 404);
        if (order.Status == 3)
            throw new DomainException("已完成的订单不能取消");

        order.Status = 4;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = currentUserName;
        await _orderRepo.UpdateAsync(order, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, string currentUserName, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("订单不存在", 404);
        order.IsDeleted = true;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = currentUserName;
        await _orderRepo.UpdateAsync(order, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
