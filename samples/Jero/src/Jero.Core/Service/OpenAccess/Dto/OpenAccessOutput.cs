namespace Jero.Core.Service;

public class OpenAccessOutput : SysOpenAccess
{
    /// <summary>
    /// 绑定用户账号
    /// </summary>
    public string BindUserAccount { get; set; } = string.Empty;
    /// <summary>
    /// 绑定租户名称
    /// </summary>
    public string BindTenantName { get; set; } = string.Empty;
}