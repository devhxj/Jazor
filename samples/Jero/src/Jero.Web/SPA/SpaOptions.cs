using Furion.ConfigurableOptions;

namespace Jero.Web;

/// <summary>
/// SPA 的 API 地址和登录配置。
/// </summary>
[OptionsSettings("FrontendRuntime")]
public sealed class SpaOptions : IConfigurableOptions
{
    /// <summary>
    /// 前端运行时 API 基地址。
    /// 支持相对路径（如 /api）或绝对地址（如 https://api.example.com）。
    /// </summary>
    public string ApiUrl { get; set; } = "/api";

    /// <summary>
    /// 第三方登录运行时配置
    /// </summary>
    public DingTalkLoginOptions DingTalk { get; set; } = new();
}

/// <summary>
/// 钉钉运行时配置选项
/// </summary>
public sealed class DingTalkLoginOptions
{
    /// <summary>
    /// 钉钉 ClientId。为空时回退到 DingTalk:ClientId
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// 钉钉 CorpId。为空时回退到 DingTalk:AppId
    /// </summary>
    public string CorpId { get; set; } = string.Empty;
}
