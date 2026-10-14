namespace TestService.Application.Dtos;

public class SalesOrderDto
{
    public Guid Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int Status { get; set; }
    public string? Remark { get; set; }
    public List<SalesOrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class SalesOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}

public class SalesOrderCreateRequest
{
    public Guid CustomerId { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Remark { get; set; }
    public List<SalesOrderItemRequest> Items { get; set; } = new();
}

public class SalesOrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; } // 缺省取商品价
}

public class SalesOrderQuery
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
    public int? Status { get; set; }
    public Guid? CustomerId { get; set; }
}
