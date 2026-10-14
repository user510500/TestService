using TestService.Application.Common;

namespace TestService.Application.Dtos;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Address { get; set; }
    public int Level { get; set; }
    public int Status { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Address { get; set; }
    public int Level { get; set; } = 1;
    public string? Remark { get; set; }
}

public class CustomerUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Address { get; set; }
    public int Level { get; set; }
    public int Status { get; set; }
    public string? Remark { get; set; }
}

public class CustomerQuery : PagedRequest
{
    public int? Level { get; set; }
    public int? Status { get; set; }
}
