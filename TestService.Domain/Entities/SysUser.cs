using TestService.Domain.Common;

namespace TestService.Domain.Entities;

/// <summary>
/// 系统用户。
/// </summary>
public class SysUser : FullAuditedEntity
{
    /// <summary>登录名（唯一）</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>密码哈希（BCrypt）</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>盐（BCrypt 内嵌于 hash，可选保留）</summary>
    public string Salt { get; set; } = string.Empty;

    /// <summary>真实姓名</summary>
    public string? RealName { get; set; }

    /// <summary>邮箱</summary>
    public string? Email { get; set; }

    /// <summary>手机号</summary>
    public string? Phone { get; set; }

    /// <summary>状态：0=禁用 1=启用</summary>
    public int Status { get; set; } = 1;

    /// <summary>最后登录时间</summary>
    public DateTime? LastLoginTime { get; set; }

    /// <summary>导航属性：用户拥有的角色</summary>
    public ICollection<SysUserRole> UserRoles { get; set; } = new List<SysUserRole>();
}
