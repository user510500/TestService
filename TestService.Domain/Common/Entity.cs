namespace TestService.Domain.Common;

/// <summary>
/// 实体基类：所有领域实体继承该类，统一主键与基础审计字段。
/// </summary>
public abstract class Entity
{
    /// <summary>主键（Guid）</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>创建人</summary>
    public string? CreatedBy { get; set; }

    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>更新人</summary>
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// 软删除实体基类。
/// </summary>
public abstract class FullAuditedEntity : Entity
{
    /// <summary>是否已删除（软删除）</summary>
    public bool IsDeleted { get; set; }
}
