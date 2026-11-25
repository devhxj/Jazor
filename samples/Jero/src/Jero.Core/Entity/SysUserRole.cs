namespace Jero.Core;

/// <summary>
/// 系统用户角色表
/// </summary>
[SugarTable(null, "系统用户角色表")]
[SysTable]
public class SysUserRole : EntityBaseId
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
    /// 角色Id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id")]
    public long RoleId { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(RoleId))]
    public SysRole? SysRole { get; set; }
}
