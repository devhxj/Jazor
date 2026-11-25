namespace Jero.Plugin.GoView.Service;

/// <summary>
/// 登录输入
/// </summary>
public class GoViewLoginInput
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 租户
    /// </summary>
    public long? TenantId { get; set; }
}
