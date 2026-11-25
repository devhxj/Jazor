namespace Jero.Core;

/// <summary>
/// 系统用户菜单快捷导航表
/// </summary>
[SugarTable(null, "系统用户菜单快捷导航表")]
[SysTable]
public partial class SysUserMenu : EntityBaseId
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public SysUser? SysUser { get; set; }

    /// <summary>
    /// 菜单Id
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单Id")]
    public long MenuId { get; set; }

    /// <summary>
    /// 菜单
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(MenuId))]
    public SysMenu? SysMenu { get; set; }
}
