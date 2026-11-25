namespace Jero.Core.Service;

/// <summary>
/// 还原输入参数
/// </summary>
public class RestoreInput
{
    /// <summary>
    /// 文件名
    /// </summary>
    [Required(ErrorMessage = "文件名不能为空")]
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// WebHook输入参数
/// </summary>
public class WebHookInput
{
    /// <summary>
    /// 密钥
    /// </summary>
    [Required(ErrorMessage = "密钥不能为空")]
    public string Key { get; set; } = string.Empty;
}