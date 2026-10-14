using TestService.Domain.Common;

namespace TestService.Domain.Entities;

/// <summary>
/// 销售订单头。
/// </summary>
public class SoSalesOrder : FullAuditedEntity
{
    /// <summary>订单号</summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>客户 ID</summary>
    public Guid CustomerId { get; set; }

    /// <summary>下单日期</summary>
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>总金额</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>优惠金额</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>实付金额</summary>
    public decimal FinalAmount { get; set; }

    /// <summary>状态：1=草稿 2=已确认 3=已完成 4=已取消</summary>
    public int Status { get; set; } = 1;

    /// <summary>备注</summary>
    public string? Remark { get; set; }

    public CusCustomer? Customer { get; set; }
    public ICollection<SoSalesOrderItem> Items { get; set; } = new List<SoSalesOrderItem>();
}
