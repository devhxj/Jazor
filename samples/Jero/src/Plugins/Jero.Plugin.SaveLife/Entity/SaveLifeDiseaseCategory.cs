namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机疾病分类表
/// </summary>
[SugarTable("sl_disease_category", "医线生机疾病分类表")]
public class SaveLifeDiseaseCategory : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 分类名称
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "分类名称", Length = 64)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分类描述
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "分类描述", Length = 256)]
    public string? Description { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "图标", Length = 64)]
    public string? Icon { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int SortOrder { get; set; } = 100;

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;
}
