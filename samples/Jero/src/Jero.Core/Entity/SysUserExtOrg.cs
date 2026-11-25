namespace Jero.Core;

/// <summary>
/// 系统用户扩展机构表
/// </summary>
[SugarTable(null, "系统用户扩展机构表")]
[SysTable]
public partial class SysUserExtOrg : EntityBaseId
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
    /// 机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id")]
    public long OrgId { get; set; }

    /// <summary>
    /// 机构
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Navigate(NavigateType.OneToOne, nameof(OrgId))]
    public SysOrg? SysOrg { get; set; }

    /// <summary>
    /// 职位Id
    /// </summary>
    [SugarColumn(ColumnDescription = "职位Id")]
    public long PosId { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Navigate(NavigateType.OneToOne, nameof(PosId))]
    public SysPos? SysPos { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    [SugarColumn(ColumnDescription = "工号", Length = 32)]
    [MaxLength(32)]
    public string? JobNum { get; set; }

    /// <summary>
    /// 职级
    /// </summary>
    [SugarColumn(ColumnDescription = "职级", Length = 32)]
    [MaxLength(32)]
    public string? PosLevel { get; set; }

    /// <summary>
    /// 入职日期
    /// </summary>
    [SugarColumn(ColumnDescription = "入职日期")]
    public DateTime? JoinDate { get; set; }
}
