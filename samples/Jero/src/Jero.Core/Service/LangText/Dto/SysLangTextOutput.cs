namespace Jero.Core;

/// <summary>
/// 翻译表输出参数
/// </summary>
public class SysLangTextOutput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 所属实体名
    /// </summary>
    public string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// 所属实体ID
    /// </summary>
    public long EntityId { get; set; }

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

/// <summary>
/// 翻译表数据导入模板实体
/// </summary>
public class ExportSysLangTextOutput : ImportSysLangTextInput
{
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public override string Error { get; set; } = string.Empty;
}