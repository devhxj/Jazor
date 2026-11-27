using SqlSugar;

namespace Jero.Application.Modules.GoView;

/// <summary>
/// GoView 项目数据表
/// </summary>
[SugarTable(null, "GoView 项目数据表")]
[SysTable]
public class GoViewProData : EntityBaseTenant
{
    /// <summary>
    /// 项目内容
    /// </summary>
    [SugarColumn(ColumnDescription = "项目内容", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Content { get; set; }

    /// <summary>
    /// 预览图片
    /// </summary>
    [SugarColumn(ColumnDescription = "预览图片", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? IndexImageData { get; set; }

    /// <summary>
    /// 背景图片
    /// </summary>
    [SugarColumn(ColumnDescription = "背景图片", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? BackGroundImageData { get; set; }
}