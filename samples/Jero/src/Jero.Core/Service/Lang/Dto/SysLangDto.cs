namespace Jero.Core;

/// <summary>
/// 语言输出参数
/// </summary>
public class SysLangDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 语言名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// ISO 语言代码
    /// </summary>
    public string IsoCode { get; set; } = string.Empty;
    /// <summary>
    /// URL 语言代码
    /// </summary>
    public string UrlCode { get; set; } = string.Empty;
    /// <summary>
    /// 书写方向
    /// </summary>
    public DirectionEnum Direction { get; set; }

    /// <summary>
    /// 日期格式
    /// </summary>
    public string DateFormat { get; set; } = string.Empty;
    /// <summary>
    /// 时间格式
    /// </summary>
    public string TimeFormat { get; set; } = string.Empty;
    /// <summary>
    /// 每周起始日
    /// </summary>
    public WeekEnum WeekStart { get; set; }

    /// <summary>
    /// 分组符号
    /// </summary>
    public string Grouping { get; set; } = string.Empty;
    /// <summary>
    /// 小数点符号
    /// </summary>
    public string DecimalPoint { get; set; } = string.Empty;
    /// <summary>
    /// 千分位分隔符
    /// </summary>
    public string? ThousandsSep { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    public long? CreateUserId { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    public long? UpdateUserId { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
}