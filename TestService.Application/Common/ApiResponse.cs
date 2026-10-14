namespace TestService.Application.Common;

/// <summary>
/// 统一 API 响应包装。
/// 按部门规范：所有 API 响应必须使用 ApiResponse&lt;T&gt;，禁止自定义响应类。
/// </summary>
public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static ApiResponse<T> Ok(T data, string message = "操作成功") => new() { Code = 0, Message = message, Data = data };
    public static ApiResponse<T> Fail(string message, int code = 1) => new() { Code = code, Message = message, Data = default };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "操作成功") => new() { Code = 0, Message = message };
    public static new ApiResponse Fail(string message, int code = 1) => new() { Code = code, Message = message };
}
