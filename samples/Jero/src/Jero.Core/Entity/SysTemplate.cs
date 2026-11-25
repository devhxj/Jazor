namespace Jero.Core;

/// <summary>
/// 系统模板表
/// </summary>
[SysTable]
[SugarTable(null, "系统模板表")]
[SugarIndex("index_{table}_C", nameof(Code), OrderByType.Asc, IsUnique = true)]
[SugarIndex("index_{table}_G", nameof(GroupName), OrderByType.Asc)]
public partial class SysTemplate : EntityBaseTenant
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "名称", Length = 128)]
    public virtual string Name { get; set; } = string.Empty;
    /// <summary>
    /// 分组名称
    /// </summary>
    [SugarColumn(ColumnDescription = "分组名称")]
    public virtual TemplateTypeEnum Type { get; set; }

    /// <summary>
    /// 编码
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "编码", Length = 128)]
    public virtual string Code { get; set; } = string.Empty;
    /// <summary>
    /// 分组名称
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "分组名称", Length = 32)]
    public virtual string GroupName { get; set; } = string.Empty;
    /// <summary>
    /// 模板内容
    /// </summary>
    [SugarColumn(ColumnDescription = "模板内容", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public virtual string Content { get; set; } = string.Empty;
    /// <summary>
    /// 备注
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "备注", Length = 128)]
    public virtual string? Remark { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public virtual int OrderNo { get; set; } = 100;
}