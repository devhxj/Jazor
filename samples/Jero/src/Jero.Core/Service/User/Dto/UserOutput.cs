namespace Jero.Core.Service;

public class UserOutput : SysUser
{
    /// <summary>
    /// 机构名称
    /// </summary>
    public string OrgName { get; set; } = string.Empty;
    /// <summary>
    /// 职位名称
    /// </summary>
    public string PosName { get; set; } = string.Empty;
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    /// <summary>
    /// 域用户
    /// </summary>
    public string DomainAccount { get; set; } = string.Empty;
}