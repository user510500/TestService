using TestService.Domain.Common;

namespace TestService.Domain.Entities;

/// <summary>
/// 系统角色。
/// </summary>
public class SysRole : FullAuditedEntity
{
    /// <summary>角色名（中文）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>角色编码（唯一，如 admin / sales）</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>状态：0=禁用 1=启用</summary>
    public int Status { get; set; } = 1;

    public ICollection<SysUserRole> UserRoles { get; set; } = new List<SysUserRole>();
}
