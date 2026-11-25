namespace Jero.Core;

/// <summary>
/// 系统用户域配置表
/// </summary>
[SugarTable(null, "系统用户域配置表")]
[SysTable]
[SugarIndex("index_{table}_A", nameof(Account), OrderByType.Asc)]
[SugarIndex("index_{table}_U", nameof(UserId), OrderByType.Asc)]
public class SysUserLdap : EntityBaseTenantId
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 域账号
    /// AD域对应sAMAccountName
    /// Ldap对应uid
    /// </summary>
    [SugarColumn(ColumnDescription = "域账号", Length = 32)]
    [Required]
    public string Account { get; set; } = string.Empty;
    /// <summary>
    /// 域用户名
    /// </summary>
    [SugarColumn(ColumnDescription = "域用户名", Length = 32)]
    public string UserName { get; set; } = string.Empty;
    /// <summary>
    /// 对应EmployeeId(用于数据导入对照)
    /// </summary>
    [SugarColumn(ColumnDescription = "对应EmployeeId", Length = 32)]
    public string? EmployeeId { get; set; }

    /// <summary>
    /// 组织代码
    /// </summary>
    [SugarColumn(ColumnDescription = "组织代码", Length = 64)]
    public string? DeptCode { get; set; }

    /// <summary>
    /// 最后设置密码时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最后设置密码时间")]
    public DateTime? PwdLastSetTime { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [SugarColumn(ColumnDescription = "组织代码", Length = 64)]
    public string? Mail { get; set; }

    /// <summary>
    /// 检查账户是否已过期
    /// </summary>
    [SugarColumn(ColumnDescription = "检查账户是否已过期")]
    public bool AccountExpiresFlag { get; set; } = false;

    /// <summary>
    /// 密码设置是否永不过期
    /// </summary>
    [SugarColumn(ColumnDescription = "密码设置是否永不过期")]
    public bool DontExpiresFlag { get; set; } = false;

    /// <summary>
    /// DN
    /// </summary>
    [SugarColumn(ColumnDescription = "DN", Length = 512)]
    public string Dn { get; set; } = string.Empty;
}