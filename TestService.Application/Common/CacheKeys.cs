namespace TestService.Application.Common;

/// <summary>
/// 集中管理缓存 Key 命名规范。
/// 规则：{业务}:{实体}:{维度}:{ID}
/// 避免不同业务的 key 冲突，方便批量失效。
/// </summary>
public static class CacheKeys
{
    // 商品相关
    public static string ProductDetail(Guid id) => $"product:detail:{id}";
    public static string ProductList(int pageIndex, int pageSize, string? keyword, string? category, int? status)
        => $"product:list:{pageIndex}:{pageSize}:{keyword ?? "_"}:{category ?? "_"}:{status?.ToString() ?? "_"}";

    // 客户相关（占位，后续如果要加缓存用同样的命名规则）
    public static string CustomerDetail(Guid id) => $"customer:detail:{id}";
    public static string CustomerList(int pageIndex, int pageSize, string? keyword, int? level, int? status)
        => $"customer:list:{pageIndex}:{pageSize}:{keyword ?? "_"}:{level?.ToString() ?? "_"}:{status?.ToString() ?? "_"}";
}
