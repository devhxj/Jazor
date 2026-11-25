namespace Jero.Core;

/// <summary>
/// 系统域登录信息配置表
/// </summary>
[SugarTable(null, "系统域登录信息配置表")]
[SysTable]
public class SysLdap : EntityBaseTenantDel
{
    /// <summary>
    /// 主机
    /// </summary>
    [SugarColumn(ColumnDescription = "主机", Length = 128)]
    [Required]
    public virtual string Host { get; set; } = string.Empty;
    /// <summary>
    /// 端口
    /// </summary>
    [SugarColumn(ColumnDescription = "端口")]
    public virtual int Port { get; set; }

    /// <summary>
    /// 用户搜索基准
    /// </summary>
    [SugarColumn(ColumnDescription = "用户搜索基准", Length = 128)]
    [Required]
    public virtual string BaseDn { get; set; } = string.Empty;
    /// <summary>
    /// 绑定DN(有管理权限制的用户)
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定DN", Length = 128)]
    [Required]
    public virtual string BindDn { get; set; } = string.Empty;
    /// <summary>
    /// 绑定密码(有管理权限制的用户密码)
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定密码", Length = 512)]
    [Required]
    public virtual string BindPass { get; set; } = string.Empty;
    /// <summary>
    /// 用户过滤规则
    /// </summary>
    [SugarColumn(ColumnDescription = "用户过滤规则", Length = 128)]
    [Required]
    public virtual string AuthFilter { get; set; } = "sAMAccountName=%s";

    /// <summary>
    /// Ldap版本
    /// </summary>
    [SugarColumn(ColumnDescription = "Ldap版本")]
    public int Version { get; set; }

    /// <summary>
    /// 绑定域账号字段属性值
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定域账号字段属性值", Length = 32)]
    [Required]
    public virtual string BindAttrAccount { get; set; } = "sAMAccountName";

    /// <summary>
    /// 绑定用户EmployeeId属性值
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定用户EmployeeId属性值", Length = 32)]
    [Required]
    public virtual string BindAttrEmployeeId { get; set; } = "EmployeeId";

    /// <summary>
    /// 绑定Code属性值
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定对象Code属性值", Length = 64)]
    [Required]
    public virtual string BindAttrCode { get; set; } = "objectGUID";

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;
}