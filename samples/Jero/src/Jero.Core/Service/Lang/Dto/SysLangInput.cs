namespace Jero.Core;

/// <summary>
/// 语言基础输入参数
/// </summary>
public class SysLangBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }

    /// <summary>
    /// 语言名称
    /// </summary>
    [Required(ErrorMessage = "语言名称不能为空")]
    public virtual string Name { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    [Required(ErrorMessage = "语言代码不能为空")]
    public virtual string Code { get; set; } = string.Empty;
    /// <summary>
    /// ISO 语言代码
    /// </summary>
    [Required(ErrorMessage = "ISO 语言代码不能为空")]
    public virtual string IsoCode { get; set; } = string.Empty;
    /// <summary>
    /// URL 语言代码
    /// </summary>
    [Required(ErrorMessage = "URL 语言代码不能为空")]
    public virtual string UrlCode { get; set; } = string.Empty;
    /// <summary>
    /// 书写方向
    /// </summary>
    [Required(ErrorMessage = "书写方向不能为空")]
    public virtual DirectionEnum Direction { get; set; }

    /// <summary>
    /// 日期格式
    /// </summary>
    [Required(ErrorMessage = "日期格式不能为空")]
    public virtual string DateFormat { get; set; } = string.Empty;
    /// <summary>
    /// 时间格式
    /// </summary>
    [Required(ErrorMessage = "时间格式不能为空")]
    public virtual string TimeFormat { get; set; } = string.Empty;
    /// <summary>
    /// 每周起始日
    /// </summary>
    [Required(ErrorMessage = "每周起始日不能为空")]
    public virtual WeekEnum? WeekStart { get; set; }

    /// <summary>
    /// 分组符号
    /// </summary>
    [Required(ErrorMessage = "分组符号不能为空")]
    public virtual string Grouping { get; set; } = string.Empty;
    /// <summary>
    /// 小数点符号
    /// </summary>
    [Required(ErrorMessage = "小数点符号不能为空")]
    public virtual string DecimalPoint { get; set; } = string.Empty;
    /// <summary>
    /// 千分位分隔符
    /// </summary>
    public virtual string? ThousandsSep { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Required(ErrorMessage = "是否启用不能为空")]
    public virtual bool? Active { get; set; }
}

/// <summary>
/// 多语言分页查询输入参数
/// </summary>
public class PageSysLangInput : BasePageInput
{
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
    /// 是否启用
    /// </summary>
    public bool? Active { get; set; }

    /// <summary>
    /// 选中主键列表
    /// </summary>
    public List<long> SelectKeyList { get; set; } = [];
}

/// <summary>
/// 多语言增加输入参数
/// </summary>
public class AddSysLangInput
{
    /// <summary>
    /// 语言名称
    /// </summary>
    [Required(ErrorMessage = "语言名称不能为空")]
    [MaxLength(255, ErrorMessage = "语言名称字符长度不能超过255")]
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    [Required(ErrorMessage = "语言代码不能为空")]
    [MaxLength(255, ErrorMessage = "语言代码字符长度不能超过255")]
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// ISO 语言代码
    /// </summary>
    [Required(ErrorMessage = "ISO 语言代码不能为空")]
    [MaxLength(255, ErrorMessage = "ISO 语言代码字符长度不能超过255")]
    public string IsoCode { get; set; } = string.Empty;
    /// <summary>
    /// URL 语言代码
    /// </summary>
    [Required(ErrorMessage = "URL 语言代码不能为空")]
    [MaxLength(255, ErrorMessage = "URL 语言代码字符长度不能超过255")]
    public string UrlCode { get; set; } = string.Empty;
    /// <summary>
    /// 书写方向
    /// </summary>
    [Required(ErrorMessage = "书写方向不能为空")]
    public DirectionEnum Direction { get; set; }

    /// <summary>
    /// 日期格式
    /// </summary>
    [Required(ErrorMessage = "日期格式不能为空")]
    [MaxLength(255, ErrorMessage = "日期格式字符长度不能超过255")]
    public string DateFormat { get; set; } = string.Empty;
    /// <summary>
    /// 时间格式
    /// </summary>
    [Required(ErrorMessage = "时间格式不能为空")]
    [MaxLength(255, ErrorMessage = "时间格式字符长度不能超过255")]
    public string TimeFormat { get; set; } = string.Empty;
    /// <summary>
    /// 每周起始日
    /// </summary>
    [Required(ErrorMessage = "每周起始日不能为空")]
    public WeekEnum? WeekStart { get; set; }

    /// <summary>
    /// 分组符号
    /// </summary>
    [Required(ErrorMessage = "分组符号不能为空")]
    [MaxLength(255, ErrorMessage = "分组符号字符长度不能超过255")]
    public string Grouping { get; set; } = string.Empty;
    /// <summary>
    /// 小数点符号
    /// </summary>
    [Required(ErrorMessage = "小数点符号不能为空")]
    [MaxLength(255, ErrorMessage = "小数点符号字符长度不能超过255")]
    public string DecimalPoint { get; set; } = string.Empty;
    /// <summary>
    /// 千分位分隔符
    /// </summary>
    [MaxLength(255, ErrorMessage = "千分位分隔符字符长度不能超过255")]
    public string? ThousandsSep { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Required(ErrorMessage = "是否启用不能为空")]
    public bool? Active { get; set; }
}

/// <summary>
/// 多语言删除输入参数
/// </summary>
public class DeleteSysLangInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
}

/// <summary>
/// 多语言更新输入参数
/// </summary>
public class UpdateSysLangInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }

    /// <summary>
    /// 语言名称
    /// </summary>
    [Required(ErrorMessage = "语言名称不能为空")]
    [MaxLength(255, ErrorMessage = "语言名称字符长度不能超过255")]
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    [Required(ErrorMessage = "语言代码不能为空")]
    [MaxLength(255, ErrorMessage = "语言代码字符长度不能超过255")]
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// ISO 语言代码
    /// </summary>
    [Required(ErrorMessage = "ISO 语言代码不能为空")]
    [MaxLength(255, ErrorMessage = "ISO 语言代码字符长度不能超过255")]
    public string IsoCode { get; set; } = string.Empty;
    /// <summary>
    /// URL 语言代码
    /// </summary>
    [Required(ErrorMessage = "URL 语言代码不能为空")]
    [MaxLength(255, ErrorMessage = "URL 语言代码字符长度不能超过255")]
    public string UrlCode { get; set; } = string.Empty;
    /// <summary>
    /// 书写方向
    /// </summary>
    [Required(ErrorMessage = "书写方向不能为空")]
    public DirectionEnum Direction { get; set; }

    /// <summary>
    /// 日期格式
    /// </summary>
    [Required(ErrorMessage = "日期格式不能为空")]
    [MaxLength(255, ErrorMessage = "日期格式字符长度不能超过255")]
    public string DateFormat { get; set; } = string.Empty;
    /// <summary>
    /// 时间格式
    /// </summary>
    [Required(ErrorMessage = "时间格式不能为空")]
    [MaxLength(255, ErrorMessage = "时间格式字符长度不能超过255")]
    public string TimeFormat { get; set; } = string.Empty;
    /// <summary>
    /// 每周起始日
    /// </summary>
    [Required(ErrorMessage = "每周起始日不能为空")]
    public WeekEnum? WeekStart { get; set; }

    /// <summary>
    /// 分组符号
    /// </summary>
    [Required(ErrorMessage = "分组符号不能为空")]
    [MaxLength(255, ErrorMessage = "分组符号字符长度不能超过255")]
    public string Grouping { get; set; } = string.Empty;
    /// <summary>
    /// 小数点符号
    /// </summary>
    [Required(ErrorMessage = "小数点符号不能为空")]
    [MaxLength(255, ErrorMessage = "小数点符号字符长度不能超过255")]
    public string DecimalPoint { get; set; } = string.Empty;
    /// <summary>
    /// 千分位分隔符
    /// </summary>
    [MaxLength(255, ErrorMessage = "千分位分隔符字符长度不能超过255")]
    public string? ThousandsSep { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Required(ErrorMessage = "是否启用不能为空")]
    public bool? Active { get; set; }
}

/// <summary>
/// 多语言主键查询输入参数
/// </summary>
public class QueryByIdSysLangInput : DeleteSysLangInput
{
}

/// <summary>
/// 多语言数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportSysLangInput : BaseImportInput
{
    /// <summary>
    /// 语言名称
    /// </summary>
    [ImporterHeader(Name = "*语言名称")]
    [ExporterHeader("*语言名称", Format = "", Width = 25, IsBold = true)]
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    [ImporterHeader(Name = "*语言代码")]
    [ExporterHeader("*语言代码", Format = "", Width = 25, IsBold = true)]
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// ISO 语言代码
    /// </summary>
    [ImporterHeader(Name = "*ISO 语言代码")]
    [ExporterHeader("*ISO 语言代码", Format = "", Width = 25, IsBold = true)]
    public string IsoCode { get; set; } = string.Empty;
    /// <summary>
    /// URL 语言代码
    /// </summary>
    [ImporterHeader(Name = "*URL 语言代码")]
    [ExporterHeader("*URL 语言代码", Format = "", Width = 25, IsBold = true)]
    public string UrlCode { get; set; } = string.Empty;
    /// <summary>
    /// 书写方向
    /// </summary>
    [ImporterHeader(Name = "*书写方向")]
    [ExporterHeader("*书写方向", Format = "", Width = 25, IsBold = true)]
    public DirectionEnum Direction { get; set; }

    /// <summary>
    /// 日期格式
    /// </summary>
    [ImporterHeader(Name = "*日期格式")]
    [ExporterHeader("*日期格式", Format = "", Width = 25, IsBold = true)]
    public string DateFormat { get; set; } = string.Empty;
    /// <summary>
    /// 时间格式
    /// </summary>
    [ImporterHeader(Name = "*时间格式")]
    [ExporterHeader("*时间格式", Format = "", Width = 25, IsBold = true)]
    public string TimeFormat { get; set; } = string.Empty;
    /// <summary>
    /// 每周起始日
    /// </summary>
    [ImporterHeader(Name = "*每周起始日")]
    [ExporterHeader("*每周起始日", Format = "", Width = 25, IsBold = true)]
    public WeekEnum? WeekStart { get; set; }

    /// <summary>
    /// 分组符号
    /// </summary>
    [ImporterHeader(Name = "*分组符号")]
    [ExporterHeader("*分组符号", Format = "", Width = 25, IsBold = true)]
    public string Grouping { get; set; } = string.Empty;
    /// <summary>
    /// 小数点符号
    /// </summary>
    [ImporterHeader(Name = "*小数点符号")]
    [ExporterHeader("*小数点符号", Format = "", Width = 25, IsBold = true)]
    public string DecimalPoint { get; set; } = string.Empty;
    /// <summary>
    /// 千分位分隔符
    /// </summary>
    [ImporterHeader(Name = "千分位分隔符")]
    [ExporterHeader("千分位分隔符", Format = "", Width = 25, IsBold = true)]
    public string? ThousandsSep { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [ImporterHeader(Name = "*是否启用")]
    [ExporterHeader("*是否启用", Format = "", Width = 25, IsBold = true)]
    public bool? Active { get; set; }
}