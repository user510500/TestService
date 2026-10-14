using TestService.Application.Common;

namespace TestService.Application.Dtos;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Unit { get; set; } = "件";
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQty { get; set; }
    public int Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Unit { get; set; } = "件";
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQty { get; set; }
    public string? Description { get; set; }
}

public class ProductUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Unit { get; set; } = "件";
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQty { get; set; }
    public int Status { get; set; }
    public string? Description { get; set; }
}

public class ProductQuery : PagedRequest
{
    public string? Category { get; set; }
    public int? Status { get; set; }
}
