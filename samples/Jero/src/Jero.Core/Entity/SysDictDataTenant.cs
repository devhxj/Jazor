namespace Jero.Core;

/// <summary>
/// 系统租户字典值表
/// </summary>
[SugarTable(null, "系统租户字典值表")]
[SysTable]
[SugarIndex("index_{table}_TV", nameof(DictTypeId), OrderByType.Asc, nameof(Value), OrderByType.Asc)]
public partial class SysDictDataTenant : SysDictData, ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? TenantId { get; set; }
}