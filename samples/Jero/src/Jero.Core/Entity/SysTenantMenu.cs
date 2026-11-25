namespace Jero.Core;

/// <summary>
/// 系统租户菜单表
/// </summary>
[SysTable]
[SugarTable(null, "系统租户菜单表")]
public class SysTenantMenu : EntityBaseId
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long TenantId { get; set; }

    /// <summary>
    /// 菜单Id
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单Id")]
    public long MenuId { get; set; }
}