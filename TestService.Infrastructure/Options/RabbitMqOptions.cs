namespace TestService.Infrastructure.Options;

/// <summary>RabbitMQ 配置</summary>
public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    /// <summary>是否启动后台消费者（一般只在主服务节点启用）</summary>
    public bool EnableConsumer { get; set; } = false;
}
