namespace Jero.Core;

/// <summary>
/// 系统字典值表
/// </summary>
[SugarTable(null, "系统字典值表")]
[SysTable]
[SugarIndex("index_{table}_TV", nameof(DictTypeId), OrderByType.Asc, nameof(Value), OrderByType.Asc, IsUnique = true)]
public partial class SysDictData : EntityBase
{
    /// <summary>
    /// 字典类型Id
    /// </summary>
    [SugarColumn(ColumnDescription = "字典类型Id")]
    public long DictTypeId { get; set; }

    /// <summary>
    /// 字典类型
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Navigate(NavigateType.OneToOne, nameof(DictTypeId))]
    public SysDictType? DictType { get; set; }

    /// <summary>
    /// 显示文本
    /// </summary>
    [SugarColumn(ColumnDescription = "显示文本", Length = 256)]
    [Required, MaxLength(256)]
    public virtual string Label { get; set; } = string.Empty;
    /// <summary>
    /// 值
    /// </summary>
    [SugarColumn(ColumnDescription = "值", Length = 256)]
    [Required, MaxLength(256)]
    public virtual string Value { get; set; } = string.Empty;
    /// <summary>
    /// 编码
    /// </summary>
    /// <remarks>
    /// </remarks>
    [SugarColumn(ColumnDescription = "编码", Length = 256)]
    public virtual string? Code { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 256)]
    [MaxLength(256)]
    public virtual string? Name { get; set; }

    /// <summary>
    /// 显示样式-标签颜色
    /// </summary>
    [SugarColumn(ColumnDescription = "显示样式-标签颜色", Length = 16)]
    [MaxLength(16)]
    public string? TagType { get; set; }

    /// <summary>
    /// 显示样式-Style(控制显示样式)
    /// </summary>
    [SugarColumn(ColumnDescription = "显示样式-Style", Length = 512)]
    [MaxLength(512)]
    public string? StyleSetting { get; set; }

    /// <summary>
    /// 显示样式-Class(控制显示样式)
    /// </summary>
    [SugarColumn(ColumnDescription = "显示样式-Class", Length = 512)]
    [MaxLength(512)]
    public string? ClassSetting { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "100")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 2048)]
    [MaxLength(2048)]
    public string? Remark { get; set; }

    /// <summary>
    /// 拓展数据(保存业务功能的配置项)
    /// </summary>
    [SugarColumn(ColumnDescription = "拓展数据(保存业务功能的配置项)", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? ExtData { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态", DefaultValue = "1")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;
}
