namespace Jero.Core;

[SugarTable(null, "语言配置")]
[SysTable]
[SugarIndex("index_{table}_N", nameof(Name), OrderByType.Asc)]
[SugarIndex("index_{table}_C", nameof(Code), OrderByType.Asc)]
public class SysLang : EntityBase
{
    /// <summary>
    /// 语言名称
    /// </summary>
    [SugarColumn(ColumnDescription = "语言名称")]
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码（如 zh-CN）
    /// </summary>
    [SugarColumn(ColumnDescription = "语言代码")]
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// ISO 语言代码
    /// </summary>
    [SugarColumn(ColumnDescription = "ISO 语言代码")]
    public string IsoCode { get; set; } = string.Empty;
    /// <summary>
    /// URL 语言代码
    /// </summary>
    [SugarColumn(ColumnDescription = "URL 语言代码")]
    public string UrlCode { get; set; } = string.Empty;
    /// <summary>
    /// 书写方向（1=从左到右，2=从右到左）
    /// </summary>
    [SugarColumn(ColumnDescription = "书写方向", DefaultValue = "1")]
    public DirectionEnum Direction { get; set; } = DirectionEnum.Ltr;

    /// <summary>
    /// 日期格式（如 YYYY-MM-DD）
    /// </summary>
    [SugarColumn(ColumnDescription = "日期格式")]
    public string DateFormat { get; set; } = string.Empty;
    /// <summary>
    /// 时间格式（如 HH:MM:SS）
    /// </summary>
    [SugarColumn(ColumnDescription = "时间格式")]
    public string TimeFormat { get; set; } = string.Empty;
    /// <summary>
    /// 每周起始日（如 0=星期日，1=星期一）
    /// </summary>
    [SugarColumn(ColumnDescription = "每周起始日", DefaultValue = "7")]
    public WeekEnum WeekStart { get; set; } = WeekEnum.Sunday;

    /// <summary>
    /// 分组符号（如 ,）
    /// </summary>
    [SugarColumn(ColumnDescription = "分组符号")]
    public string Grouping { get; set; } = string.Empty;
    /// <summary>
    /// 小数点符号
    /// </summary>
    [SugarColumn(ColumnDescription = "小数点符号")]
    public string DecimalPoint { get; set; } = string.Empty;
    /// <summary>
    /// 千分位分隔符
    /// </summary>
    [SugarColumn(ColumnDescription = "千分位分隔符")]
    public string? ThousandsSep { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnDescription = "是否启用")]
    public bool Active { get; set; }
}