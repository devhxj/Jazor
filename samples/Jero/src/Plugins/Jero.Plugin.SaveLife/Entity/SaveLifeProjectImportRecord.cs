namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机项目导入记录表
/// </summary>
[SugarTable("sl_project_import_record", "医线生机项目导入记录表")]
[SugarIndex("index_{table}_S", nameof(Status), OrderByType.Asc)]
public class SaveLifeProjectImportRecord : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 导入类型
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "导入类型", Length = 64)]
    public string? ImportType { get; set; }

    /// <summary>
    /// 输入方式
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "输入方式", Length = 64)]
    public string? InputMode { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "状态", Length = 32)]
    public string Status { get; set; } = "pending";

    /// <summary>
    /// 关联项目 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "关联项目 Id")]
    public long? ProjectId { get; set; }

    /// <summary>
    /// 关联项目名称
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "关联项目名称", Length = 256)]
    public string? ProjectTitle { get; set; }

    /// <summary>
    /// 原始输入
    /// </summary>
    [SugarColumn(ColumnDescription = "原始输入", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? SourceText { get; set; }

    /// <summary>
    /// 处理结果
    /// </summary>
    [SugarColumn(ColumnDescription = "处理结果", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? ResultMessage { get; set; }

    /// <summary>
    /// 总条数
    /// </summary>
    [SugarColumn(ColumnDescription = "总条数")]
    public int TotalCount { get; set; }

    /// <summary>
    /// 成功条数
    /// </summary>
    [SugarColumn(ColumnDescription = "成功条数")]
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败条数
    /// </summary>
    [SugarColumn(ColumnDescription = "失败条数")]
    public int FailedCount { get; set; }
}
