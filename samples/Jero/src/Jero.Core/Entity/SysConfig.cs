namespace Jero.Core;

/// <summary>
/// 系统配置参数表
/// </summary>
[SugarTable(null, "系统配置参数表")]
[SysTable]
[SugarIndex("index_{table}_N", nameof(Name), OrderByType.Asc)]
[SugarIndex("index_{table}_C", nameof(Code), OrderByType.Asc, IsUnique = true)]
public partial class SysConfig : EntityBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string Name { get; set; } = string.Empty;
    /// <summary>
    /// 编码
    /// </summary>
    [SugarColumn(ColumnDescription = "编码", Length = 64)]
    [MaxLength(64)]
    public string? Code { get; set; }

    /// <summary>
    /// 参数值
    /// </summary>
    [SugarColumn(ColumnDescription = "参数值", Length = 512)]
    [MaxLength(512)]
    [IgnoreUpdateSeedColumn]
    public string? Value { get; set; }

    /// <summary>
    /// 是否是内置参数（Y-是，N-否）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否是内置参数", DefaultValue = "1")]
    public YesNoEnum SysFlag { get; set; } = YesNoEnum.Y;

    /// <summary>
    /// 分组编码
    /// </summary>
    [SugarColumn(ColumnDescription = "分组编码", Length = 64)]
    [MaxLength(64)]
    public string? GroupCode { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "100")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 256)]
    [MaxLength(256)]
    public string? Remark { get; set; }
}