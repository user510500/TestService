using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using TestService.Application.Common;
using TestService.Application.Dtos;
using TestService.Application.Interfaces;
using TestService.Domain.Entities;
using TestService.Domain.Exceptions;
using TestService.Domain.Interfaces;

namespace TestService.Application.Services;

/// <summary>客户服务实现。</summary>
public class CustomerService : ICustomerService
{
    private readonly IRepository<CusCustomer> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IRepository<CusCustomer> repo, IUnitOfWork uow, ILogger<CustomerService> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<PagedResult<CustomerDto>> QueryAsync(CustomerQuery query, CancellationToken cancellationToken = default)
    {
        Expression<Func<CusCustomer, bool>> predicate = c => !c.IsDeleted;
        if (!string.IsNullOrWhiteSpace(query.Keyword))
            predicate = c => !c.IsDeleted && (c.Code.Contains(query.Keyword) || c.Name.Contains(query.Keyword));
        if (query.Level.HasValue)
            predicate = predicate.And(c => c.Level == query.Level.Value);
        if (query.Status.HasValue)
            predicate = predicate.And(c => c.Status == query.Status.Value);

        var total = await _repo.CountAsync(predicate, cancellationToken);
        var all = await _repo.GetListAsync(predicate, cancellationToken);
        var items = all
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                ContactName = c.ContactName,
                ContactPhone = c.ContactPhone,
                ContactEmail = c.ContactEmail,
                Address = c.Address,
                Level = c.Level,
                Status = c.Status,
                Remark = c.Remark,
                CreatedAt = c.CreatedAt
            })
            .ToList();

        return new PagedResult<CustomerDto>
        {
            Items = items,
            Total = total,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var c = await _repo.GetByIdAsync(id, cancellationToken);
        return c is null || c.IsDeleted ? null : Map(c);
    }

    public async Task<Guid> CreateAsync(CustomerCreateRequest request, string currentUserName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new DomainException("编码和名称���能为空");

        if (await _repo.ExistsAsync(c => c.Code == request.Code, cancellationToken))
            throw new DomainException("客户编码已存在");

        var entity = new CusCustomer
        {
            Code = request.Code,
            Name = request.Name,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail,
            Address = request.Address,
            Level = request.Level,
            Status = 1,
            Remark = request.Remark,
            CreatedBy = currentUserName
        };
        await _repo.AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("创建客户 {Code}", entity.Code);
        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, CustomerUpdateRequest request, string currentUserName, CancellationToken cancellationToken = default)
    {
        var c = await _repo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("客户不存在", 404);
        if (c.IsDeleted) throw new DomainException("客户已删除", 404);

        c.Name = request.Name;
        c.ContactName = request.ContactName;
        c.ContactPhone = request.ContactPhone;
        c.ContactEmail = request.ContactEmail;
        c.Address = request.Address;
        c.Level = request.Level;
        c.Status = request.Status;
        c.Remark = request.Remark;
        c.UpdatedAt = DateTime.UtcNow;
        c.UpdatedBy = currentUserName;
        await _repo.UpdateAsync(c, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, string currentUserName, CancellationToken cancellationToken = default)
    {
        var c = await _repo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("客户不存在", 404);
        c.IsDeleted = true;
        c.UpdatedAt = DateTime.UtcNow;
        c.UpdatedBy = currentUserName;
        await _repo.UpdateAsync(c, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    private static CustomerDto Map(CusCustomer c) => new()
    {
        Id = c.Id, Code = c.Code, Name = c.Name,
        ContactName = c.ContactName, ContactPhone = c.ContactPhone, ContactEmail = c.ContactEmail,
        Address = c.Address, Level = c.Level, Status = c.Status, Remark = c.Remark, CreatedAt = c.CreatedAt
    };
}

