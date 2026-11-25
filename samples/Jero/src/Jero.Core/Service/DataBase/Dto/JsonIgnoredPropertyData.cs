namespace Jero.Core.Service;

/// <summary>
/// 保存标注了JsonIgnore的Property的值信息
/// </summary>
public class JsonIgnoredPropertyData
{
    /// <summary>
    /// 记录索引
    /// </summary>
    public int RecordIndex { get; set; }

    /// <summary>
    /// 属性名
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 属性值描述
    /// </summary>
    public string Value { get; set; } = string.Empty;
}