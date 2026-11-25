namespace Jero.Core;

/// <summary>
/// 事件总线配置选项
/// </summary>
public sealed class EventBusOptions : IConfigurableOptions
{
    /// <summary>
    /// RabbitMQ
    /// </summary>
    public RabbitMQSettings RabbitMQ { get; set; } = new();
}

/// <summary>
/// RabbitMQ
/// </summary>
public sealed class RabbitMQSettings
{
    /// <summary>
    /// 账号
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// 主机
    /// </summary>
    public string HostName { get; set; } = string.Empty;
    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; }
}