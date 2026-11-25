namespace Jero.Plugin.DingTalk;

public sealed class DingTalkOptions : IConfigurableOptions
{
    /// <summary>
    /// AppId
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// AgentId
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// 原 AppKey 和 SuiteKey
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// 原 AppSecret 和 SuiteSecret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}
