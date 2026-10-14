using TestService.Domain.Common;

namespace TestService.Domain.Entities;

/// <summary>
/// 销售订单明细（行项）。
/// </summary>
public class SoSalesOrderItem : FullAuditedEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }

    /// <summary>数量</summary>
    public int Quantity { get; set; }

    /// <summary>成交单价</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>小计金额</summary>
    public decimal SubTotal { get; set; }

    public SoSalesOrder? Order { get; set; }
    public ProProduct? Product { get; set; }
}
