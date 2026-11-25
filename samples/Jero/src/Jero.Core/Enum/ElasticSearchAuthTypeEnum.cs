namespace Jero.Core;

/// <summary>
/// ES认证类型枚举
/// <para>https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/_options_on_elasticsearchclientsettings.html</para>
/// </summary>
[Description("ES认证类型枚举")]
public enum ElasticSearchAuthTypeEnum
{
    /// <summary>
    /// BasicAuthentication
    /// </summary>
    [Description("BasicAuthentication")]
    Basic = 1,

    /// <summary>
    /// ApiKey
    /// </summary>
    [Description("ApiKey")]
    ApiKey = 2,

    /// <summary>
    /// Base64ApiKey
    /// </summary>
    [Description("Base64ApiKey")]
    Base64ApiKey = 3,

    /// <summary>
    /// 不验证
    /// </summary>
    [Description("None")]
    None = 4
}