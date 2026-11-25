namespace Jero.Core.Service;

/// <summary>
/// 系统角色菜单
/// </summary>
public class RoleMenuInput : BaseIdInput
{
    /// <summary>
    /// 同步角色Id集合
    /// </summary>
    public List<long> RoleIdList { get; set; } = [];
    /// <summary>
    /// 菜单Id集合
    /// </summary>
    public List<long> MenuIdList { get; set; } = [];
}