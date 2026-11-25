namespace Jero.Core;

/// <summary>
/// 缓存配置选项
/// </summary>
public sealed class CacheOptions : IConfigurableOptions<CacheOptions>
{
    /// <summary>
    /// 缓存前缀
    /// </summary>
    public string Prefix { get; set; } = string.Empty;
    /// <summary>
    /// 缓存类型
    /// </summary>
    public string CacheType { get; set; } = string.Empty;
    /// <summary>
    /// Redis缓存
    /// </summary>
    public RedisOption Redis { get; set; } = new();
    public void PostConfigure(CacheOptions options, IConfiguration configuration)
    {
        options.Prefix = string.IsNullOrWhiteSpace(options.Prefix) ? "" : options.Prefix.Trim();
    }
}

/// <summary>
/// Redis缓存
/// </summary>
public sealed class RedisOption : RedisOptions
{
    /// <summary>
    /// 最大消息大小
    /// </summary>
    public int MaxMessageSize { get; set; }
}

/// <summary>
/// 集群配置选项
/// </summary>
public sealed class ClusterOptions : IConfigurableOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 服务器标识
    /// </summary>
    public string ServerId { get; set; } = string.Empty;
    /// <summary>
    /// 服务器IP
    /// </summary>
    public string ServerIp { get; set; } = string.Empty;
    /// <summary>
    /// SignalR配置
    /// </summary>
    public ClusterSignalR SignalR { get; set; } = new();
    /// <summary>
    /// 数据保护key
    /// </summary>
    public string DataProtecteKey { get; set; } = string.Empty;
    /// <summary>
    /// 是否哨兵模式
    /// </summary>
    public bool IsSentinel { get; set; }

    /// <summary>
    /// 哨兵配置
    /// </summary>
    public StackExchangeSentinelConfig SentinelConfig { get; set; } = new();
}

/// <summary>
/// 集群SignalR配置
/// </summary>
public sealed class ClusterSignalR
{
    /// <summary>
    /// Redis连接字符串
    /// </summary>
    public string RedisConfiguration { get; set; } = string.Empty;
    /// <summary>
    /// 缓存前缀
    /// </summary>
    public string ChannelPrefix { get; set; } = string.Empty;
}

/// <summary>
/// 哨兵配置
/// </summary>
public sealed class StackExchangeSentinelConfig
{
    /// <summary>
    /// master名称
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    /// <summary>
    /// master访问密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// 哨兵访问密码
    /// </summary>
    public string SentinelPassword { get; set; } = string.Empty;
    /// <summary>
    /// 哨兵端口
    /// </summary>
    public List<string> EndPoints { get; set; } = [];
    /// <summary>
    /// 默认库
    /// </summary>
    public int DefaultDb { get; set; }

    /// <summary>
    /// 主前缀
    /// </summary>
    public string MainPrefix { get; set; } = string.Empty;
    /// <summary>
    /// SignalR前缀
    /// </summary>
    public string SignalRChannelPrefix { get; set; } = string.Empty;
}