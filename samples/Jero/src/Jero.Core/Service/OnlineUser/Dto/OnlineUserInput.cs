namespace Jero.Core.Service;

public class PageOnlineUserInput : BasePageInput
{
    /// <summary>
    /// 账号名称
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; } = string.Empty;
    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}