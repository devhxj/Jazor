namespace Jero.Core.Service;

/// <summary>
/// 机构树形输出
/// </summary>
public class OrgTreeOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [SugarColumn(IsTreeKey = true)]
    public long Id { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 父Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 机构子项
    /// </summary>
    public List<OrgTreeOutput> Children { get; set; } = [];
    /// <summary>
    /// 是否禁止选中
    /// </summary>
    public bool Disabled { get; set; }
}