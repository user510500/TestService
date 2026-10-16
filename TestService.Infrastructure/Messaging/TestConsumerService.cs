using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestService.Domain.Interfaces;
using TestService.Infrastructure.Options;

namespace TestService.Infrastructure.Messaging;

/// <summary>
/// 测试消费者：把收到的 RabbitMQ 消息打到日志里。
/// 仅用于本地开发调试，生产环境应该用真正的下游服务（库存/通知/审计）。
/// </summary>
public class TestConsumerService : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<TestConsumerService> _logger;

    public TestConsumerService(
        IMessageBus messageBus,
        IOptions<RabbitMqOptions> options,
        ILogger<TestConsumerService> logger)
    {
        _messageBus = messageBus;
        _options = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EnableConsumer)
        {
            _logger.LogInformation("TestConsumer 未启用 (EnableConsumer=false)，跳过订阅");
            return Task.CompletedTask;
        }

        // 订阅所有 sales.*.* 事件（Topic 通配符）
        // ⚠️ 注意：# 匹配零或多段，* 匹配正好一段
        // 我们发布的是 sales.order.created (3段)，所以要用 sales.#
        _messageBus.Subscribe<object>(
            queue: "test-service.audit",
            exchange: "erp.events",
            routingKey: "sales.#",
            handler: (msg, ct) =>
            {
                _logger.LogInformation("✓ 收到事件: {Payload}", JsonSerializer.Serialize(msg));
                return Task.CompletedTask;
            });

        _logger.LogInformation("✓ TestConsumer 已订阅 erp.events / sales.*");
        return Task.CompletedTask;
    }
}
