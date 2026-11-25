namespace Jero.Core;

/// <summary>
/// 百度翻译结果
/// </summary>
public class BaiDuTranslationResult
{
    /// <summary>
    /// 源语种
    /// </summary>
    public string From { get; set; } = string.Empty;
    /// <summary>
    /// 目标语种
    /// </summary>
    public string To { get; set; } = string.Empty;
    /// <summary>
    /// 翻译结果
    /// </summary>
    public List<TransResult> trans_result { get; set; } = [];
    /// <summary>
    /// 错误码 正常为0
    /// </summary>
    public string error_code { get; set; } = "0";

    /// <summary>
    /// 错误信息
    /// </summary>
    public string error_msg { get; set; } = String.Empty;
}

/// <summary>
/// 翻译结果
/// </summary>
public class TransResult
{
    /// <summary>
    /// 源字符
    /// </summary>
    public string Src { get; set; } = string.Empty;
    /// <summary>
    /// 目标字符
    /// </summary>
    public string Dst { get; set; } = string.Empty;
}