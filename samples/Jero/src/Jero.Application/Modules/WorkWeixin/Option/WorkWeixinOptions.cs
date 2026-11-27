using Furion.ConfigurableOptions;

namespace Jero.Application.Modules.WorkWeixin.Option;

/// <summary>
/// 企业微信配置项
/// </summary>
public class WorkWeixinOptions : IConfigurableOptions
{
    /// <summary>
    /// 企业ID
    /// </summary>
    public string CorpId { get; set; } = string.Empty;

    /// <summary>
    /// 企业微信凭证密钥
    /// </summary>
    public string CorpSecret { get; set; } = string.Empty;
}
