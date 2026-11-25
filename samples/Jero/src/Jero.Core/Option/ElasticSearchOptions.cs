namespace Jero.Core;

/// <summary>
/// ES配置选项
/// </summary>
public class ElasticSearchOptions
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// ES认证类型，可选 Basic、ApiKey、Base64ApiKey
    /// </summary>
    public ElasticSearchAuthTypeEnum AuthType { get; set; } = new();
    /// <summary>
    /// Basic认证的用户名
    /// </summary>
    public string User { get; set; } = string.Empty;
    /// <summary>
    /// Basic认证的密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// ApiKey认证的ApiId
    /// </summary>
    public string ApiId { get; set; } = string.Empty;
    /// <summary>
    /// ApiKey认证的ApiKey
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    /// <summary>
    /// Base64ApiKey认证时加密的加密字符串
    /// </summary>
    public string Base64ApiKey { get; set; } = string.Empty;
    /// <summary>
    /// ES使用Https时的证书指纹，使用证书请自行实现
    /// <para>https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/connecting.html</para>
    /// </summary>
    public string Fingerprint { get; set; } = string.Empty;
    /// <summary>
    /// 地址
    /// </summary>
    public List<string> ServerUris { get; set; } = new List<string>();

    /// <summary>
    /// 索引
    /// </summary>
    public string DefaultIndex { get; set; } = string.Empty;
}