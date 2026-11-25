namespace Jero.Core;

/// <summary>
/// 翻译表基础输入参数
/// </summary>
public class SysLangTextBaseInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public virtual long? Id { get; set; }

    /// <summary>
    /// 所属实体名
    /// </summary>
    [Required(ErrorMessage = "所属实体名不能为空")]
    public virtual string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// 所属实体ID
    /// </summary>
    [Required(ErrorMessage = "所属实体ID不能为空")]
    public virtual long? EntityId { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    [Required(ErrorMessage = "字段名不能为空")]
    public virtual string FieldName { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    [Required(ErrorMessage = "语言代码不能为空")]
    public virtual string LangCode { get; set; } = string.Empty;
    /// <summary>
    /// 翻译内容
    /// </summary>
    [Required(ErrorMessage = "翻译内容不能为空")]
    public virtual string Content { get; set; } = string.Empty;
}

/// <summary>
/// 翻译表分页查询输入参数
/// </summary>
public class PageSysLangTextInput : BasePageInput
{
    /// <summary>
    /// 所属实体名
    /// </summary>
    public string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// 所属实体ID
    /// </summary>
    public long? EntityId { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    public string FieldName { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    public string LangCode { get; set; } = string.Empty;
    /// <summary>
    /// 翻译内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>
    /// 选中主键列表
    /// </summary>
    public List<long> SelectKeyList { get; set; } = [];
}

/// <summary>
/// 翻译表增加输入参数
/// </summary>
public class AddSysLangTextInput
{
    /// <summary>
    /// 所属实体名
    /// </summary>
    [Required(ErrorMessage = "所属实体名不能为空")]
    [MaxLength(255, ErrorMessage = "所属实体名字符长度不能超过255")]
    public string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// 所属实体ID
    /// </summary>
    [Required(ErrorMessage = "所属实体ID不能为空")]
    public long? EntityId { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    [Required(ErrorMessage = "字段名不能为空")]
    [MaxLength(255, ErrorMessage = "字段名字符长度不能超过255")]
    public string FieldName { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    [Required(ErrorMessage = "语言代码不能为空")]
    [MaxLength(255, ErrorMessage = "语言代码字符长度不能超过255")]
    public string LangCode { get; set; } = string.Empty;
    /// <summary>
    /// 翻译内容
    /// </summary>
    [Required(ErrorMessage = "翻译内容不能为空")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 翻译表输入参数
/// </summary>
public class ListSysLangTextInput
{
    /// <summary>
    /// 所属实体名
    /// </summary>
    [Required(ErrorMessage = "所属实体名不能为空")]
    public string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// 所属实体ID
    /// </summary>
    [Required(ErrorMessage = "所属实体ID不能为空")]
    public long? EntityId { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    [Required(ErrorMessage = "字段名不能为空")]
    public string FieldName { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    public string LangCode { get; set; } = string.Empty;
}

/// <summary>
/// 翻译表删除输入参数
/// </summary>
public class DeleteSysLangTextInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }
}

/// <summary>
/// 翻译表更新输入参数
/// </summary>
public class UpdateSysLangTextInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long? Id { get; set; }

    /// <summary>
    /// 所属实体名
    /// </summary>
    [Required(ErrorMessage = "所属实体名不能为空")]
    [MaxLength(255, ErrorMessage = "所属实体名字符长度不能超过255")]
    public string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// 所属实体ID
    /// </summary>
    [Required(ErrorMessage = "所属实体ID不能为空")]
    public long? EntityId { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    [Required(ErrorMessage = "字段名不能为空")]
    [MaxLength(255, ErrorMessage = "字段名字符长度不能超过255")]
    public string FieldName { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    [Required(ErrorMessage = "语言代码不能为空")]
    [MaxLength(255, ErrorMessage = "语言代码字符长度不能超过255")]
    public string LangCode { get; set; } = string.Empty;
    /// <summary>
    /// 翻译内容
    /// </summary>
    [Required(ErrorMessage = "翻译内容不能为空")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 翻译表主键查询输入参数
/// </summary>
public class QueryByIdSysLangTextInput : DeleteSysLangTextInput
{
}

/// <summary>
/// 翻译表数据导入实体
/// </summary>
[ExcelImporter(SheetIndex = 1, IsOnlyErrorRows = true)]
public class ImportSysLangTextInput : BaseImportInput
{
    /// <summary>
    /// 所属实体名
    /// </summary>
    [ImporterHeader(Name = "*所属实体名")]
    [ExporterHeader("*所属实体名", Format = "", Width = 25, IsBold = true)]
    public string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// 所属实体ID
    /// </summary>
    [ImporterHeader(Name = "*所属实体ID")]
    [ExporterHeader("*所属实体ID", Format = "", Width = 25, IsBold = true)]
    public long? EntityId { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    [ImporterHeader(Name = "*字段名")]
    [ExporterHeader("*字段名", Format = "", Width = 25, IsBold = true)]
    public string FieldName { get; set; } = string.Empty;
    /// <summary>
    /// 语言代码
    /// </summary>
    [ImporterHeader(Name = "*语言代码")]
    [ExporterHeader("*语言代码", Format = "", Width = 25, IsBold = true)]
    public string LangCode { get; set; } = string.Empty;
    /// <summary>
    /// 翻译内容
    /// </summary>
    [ImporterHeader(Name = "*翻译内容")]
    [ExporterHeader("*翻译内容", Format = "", Width = 25, IsBold = true)]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
///
/// </summary>
public class AiTranslateTextInput
{
    /// <summary>
    /// 原文
    /// </summary>
    public string OriginalText { get; set; } = string.Empty;
    /// <summary>
    /// 目标语言
    /// </summary>
    public string TargetLang { get; set; } = string.Empty;
}