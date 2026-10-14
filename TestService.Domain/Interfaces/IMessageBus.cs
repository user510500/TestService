namespace TestService.Domain.Interfaces;

/// <summary>
/// 消息总线接口（领域层）。
/// 由 Infrastructure 层的 RabbitMQ 实现。
/// </summary>
public interface IMessageBus
{
    /// <summary>发布消息到指定交换机/路由键。</summary>
    Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default) where T : class;

    /// <summary>订阅（消费）某个队列上的消息。</summary>
    void Subscribe<T>(string queue, string exchange, string routingKey, Func<T, CancellationToken, Task> handler) where T : class;
}
