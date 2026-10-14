namespace TestService.Domain.Exceptions;

/// <summary>
/// 业务领域异常：用于表达业务规则被违反的场景，
/// 由全局异常过滤器捕获并转换为友好响应。
/// </summary>
public class DomainException : Exception
{
    public int Code { get; }
    public DomainException(string message, int code = 400) : base(message)
    {
        Code = code;
    }
}
