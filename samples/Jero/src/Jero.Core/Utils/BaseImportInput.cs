namespace Jero.Core;

/// <summary>
/// 数据导入输入参数
/// </summary>
public class BaseImportInput
{
    /// <summary>
    /// 记录Id
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader(IsIgnore = true)]
    public virtual long Id { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    [ImporterHeader(IsIgnore = true)]
    [ExporterHeader("错误信息", ColumnIndex = 9999, IsBold = true, IsAutoFit = true)]
    public virtual string Error { get; set; } = string.Empty;
}