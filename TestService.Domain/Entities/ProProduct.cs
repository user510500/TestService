using TestService.Domain.Common;

namespace TestService.Domain.Entities;

/// <summary>
/// 商品档案（ERP 核心主数据）。
/// </summary>
public class ProProduct : FullAuditedEntity
{
    /// <summary>商品编码（唯一）</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>商品名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>分类</summary>
    public string? Category { get; set; }

    /// <summary>计量单位</summary>
    public string Unit { get; set; } = "件";

    /// <summary>售价</summary>
    public decimal Price { get; set; }

    /// <summary>成本价</summary>
    public decimal CostPrice { get; set; }

    /// <summary>库存数量</summary>
    public int StockQty { get; set; }

    /// <summary>状态：0=下架 1=在售</summary>
    public int Status { get; set; } = 1;

    /// <summary>描述</summary>
    public string? Description { get; set; }
}
