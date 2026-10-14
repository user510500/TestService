using TestService.Domain.Common;

namespace TestService.Domain.Entities;

/// <summary>
/// 客户档案（ERP 核心主数据）。
/// </summary>
public class CusCustomer : FullAuditedEntity
{
    /// <summary>客户编码（唯一）</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>客户名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>联系人</summary>
    public string? ContactName { get; set; }

    /// <summary>联系电话</summary>
    public string? ContactPhone { get; set; }

    /// <summary>联系邮箱</summary>
    public string? ContactEmail { get; set; }

    /// <summary>地址</summary>
    public string? Address { get; set; }

    /// <summary>客户等级：1=普通 2=VIP 3=战略</summary>
    public int Level { get; set; } = 1;

    /// <summary>状态：0=禁用 1=启用</summary>
    public int Status { get; set; } = 1;

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}
