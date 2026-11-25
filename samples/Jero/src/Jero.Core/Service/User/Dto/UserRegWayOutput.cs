namespace Jero.Core.Service;

/// <summary>
/// 注册方案输出参数
/// </summary>
public class UserRegWayOutput : SysUserRegWay
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    /// <summary>
    /// 机构名称
    /// </summary>
    public string OrgName { get; set; } = string.Empty;
    /// <summary>
    /// 职位名称
    /// </summary>
    public string PosName { get; set; } = string.Empty;
}