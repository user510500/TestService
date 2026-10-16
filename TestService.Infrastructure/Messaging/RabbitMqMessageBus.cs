using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TestService.Domain.Interfaces;
using TestService.Infrastructure.Options;

namespace TestService.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ 消息总线实现。
/// 单连接 + 单 Channel，所有发布/订阅复用。
/// </summary>
public class RabbitMqMessageBus : IMessageBus, IHostedService, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqMessageBus> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public RabbitMqMessageBus(IOptions<RabbitMqOptions> options, ILogger<RabbitMqMessageBus> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private void EnsureChannel()
    {
        if (_channel is { IsOpen: true }) return;
        lock (_lock)
        {
            if (_channel is { IsOpen: true }) return;
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };
            _connection = factory.CreateConnection("TestService");
            _channel = _connection.CreateModel();
        }
    }

    public Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            EnsureChannel();
            var channel = _channel ?? throw new InvalidOperationException("Channel not initialized");
            channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true, autoDelete: false);
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;
            // 改回 mandatory=false 避免正常消息触发 return
            channel.BasicPublish(exchange, routingKey, mandatory: false, props, body);
            _logger.LogInformation("已发布消息到 {Exchange}/{RoutingKey}", exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发布 RabbitMQ 消息失败：{Exchange}/{RoutingKey}", exchange, routingKey);
        }
        return Task.CompletedTask;
    }

    public void Subscribe<T>(string queue, string exchange, string routingKey, Func<T, CancellationToken, Task> handler) where T : class
    {
        EnsureChannel();
        var channel = _channel ?? throw new InvalidOperationException("Channel not initialized");
        channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true, autoDelete: false);
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue, exchange, routingKey);
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            _logger.LogInformation("[Consumer] Received 事件触发, queue={Queue}, tag={Tag}, bodyLen={Len}",
                queue, ea.DeliveryTag, ea.Body.Length);
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<T>(json);
                if (msg is not null)
                {
                    await handler(msg, CancellationToken.None);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                else
                {
                    _logger.LogWarning("[Consumer] Deserialize 后 msg 为 null, json={Json}", json);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "消费 RabbitMQ 消息失败：{Queue}", queue);
                channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        };
        channel.BasicConsume(queue, autoAck: false, consumer);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            EnsureChannel();
            _logger.LogInformation("RabbitMQ 已连接 {Host}:{Port}", _options.HostName, _options.Port);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ 连接失败，业务功能降级");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        try { _channel?.Close(); } catch { /* ignore */ }
        try { _connection?.Close(); } catch { /* ignore */ }
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
