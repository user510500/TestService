namespace TestService.Domain.Interfaces;

/// <summary>
/// 工作单元接口：用于跨多表的业务操作统一提交。
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
