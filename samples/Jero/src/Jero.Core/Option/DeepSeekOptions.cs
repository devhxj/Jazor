namespace Jero.Core;

public sealed class DeepSeekOptions : IConfigurableOptions
{
    /// <summary>
    /// 源语言
    /// </summary>
    public string SourceLang { get; set; } = string.Empty;
    /// <summary>
    /// Api地址
    /// </summary>
    public string ApiUrl { get; set; } = string.Empty;
    /// <summary>
    /// API KEY
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}