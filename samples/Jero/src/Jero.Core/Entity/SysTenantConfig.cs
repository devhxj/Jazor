namespace Jero.Core;

/// <summary>
/// 系统租户配置参数表
/// </summary>
[SugarTable(null, "系统租户配置参数表")]
[SysTable]
public partial class SysTenantConfig : SysConfig
{
    /// <summary>
    /// 无效字段，用于忽略实体类的Value字段
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    private new string? Value { get; set; }
}