namespace TestService.Application.Common;

/// <summary>
/// 分页查询请求。
/// </summary>
public class PagedRequest
{
    private int _pageIndex = 1;
    private int _pageSize = 20;

    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 20,
            > 200 => 200,
            _ => value
        };
    }

    public string? Keyword { get; set; }
}

/// <summary>
/// 分页结果。
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Total { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}
