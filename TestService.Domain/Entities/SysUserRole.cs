using TestService.Domain.Common;

namespace TestService.Domain.Entities;

/// <summary>
/// 用户-角色关联（多对多）。
/// </summary>
public class SysUserRole : Entity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public SysUser? User { get; set; }
    public SysRole? Role { get; set; }
}
