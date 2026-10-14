namespace TestService.Domain.Common;

/// <summary>
/// 通用返回结果（领域层，不耦合 Web 层的 ApiResponse）。
/// Application 层在最终出口会再包装为 ApiResponse。
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? Message { get; protected set; }
    public int Code { get; protected set; } = 0;

    public static Result Success(string? message = null) => new() { IsSuccess = true, Message = message, Code = 0 };
    public static Result Failure(string message, int code = 1) => new() { IsSuccess = false, Message = message, Code = code };
}

public class Result<T> : Result
{
    public T? Data { get; private set; }

    public static Result<T> Success(T data, string? message = null) => new() { IsSuccess = true, Data = data, Message = message };
    public new static Result<T> Failure(string message, int code = 1) => new() { IsSuccess = false, Message = message, Code = code };
}
