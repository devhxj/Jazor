namespace Jero.Core.Service;

/// <summary>
/// 表唯一配置项
/// </summary>
public class TableUniqueConfigItem
{
    /// <summary>
    /// 字段列表
    /// </summary>
    public List<string> Columns { get; set; } = [];
    /// <summary>
    /// 描述信息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// 格式化查询条件
    /// </summary>
    /// <param name="separator">分隔符</param>
    /// <param name="format">模板字符串</param>
    /// <returns></returns>
    public string Format(string separator, string format) => string.Join(separator, Columns.Select(name => string.Format(format, name)));
}