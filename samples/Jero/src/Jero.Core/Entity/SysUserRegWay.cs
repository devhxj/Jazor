namespace Jero.Core;

/// <summary>
/// 系统用户注册方案表
/// </summary>
[SugarTable(null, "系统用户注册方案表")]
[SysTable]
public partial class SysUserRegWay : EntityBaseTenant
{
    /// <summary>
    /// 方案名称
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "方案名称", Length = 32)]
    public virtual string Name { get; set; } = string.Empty;
    /// <summary>
    /// 账号类型
    /// </summary>
    [SugarColumn(ColumnDescription = "账号类型")]
    public virtual AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.NormalUser;

    /// <summary>
    /// 注册用户默认角色
    /// </summary>
    [SugarColumn(ColumnDescription = "角色")]
    public virtual long RoleId { get; set; }

    /// <summary>
    /// 注册用户默认机构
    /// </summary>
    [SugarColumn(ColumnDescription = "机构")]
    public virtual long OrgId { get; set; }

    /// <summary>
    /// 注册用户默认职位
    /// </summary>
    [SugarColumn(ColumnDescription = "职位")]
    public virtual long PosId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "备注", Length = 128)]
    public string? Remark { get; set; }
}