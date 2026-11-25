namespace Jero.Core.Service;

/// <summary>
/// 数据库表列
/// </summary>
public class ColumnOuput
{
    /// <summary>
    /// 字段名
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;
    /// <summary>
    /// 实体的Property名
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;
    /// <summary>
    /// 字段数据长度
    /// </summary>
    public int ColumnLength { get; set; }

    /// <summary>
    /// 数据库中类型
    /// </summary>
    public string DataType { get; set; } = string.Empty;
    /// <summary>
    /// 字段数据默认值
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;
    /// <summary>
    /// 是否为主键
    /// </summary>
    public bool IsPrimarykey { get; set; }

    /// <summary>
    /// 是否允许为空
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// .NET字段类型
    /// </summary>
    public string NetType { get; set; } = string.Empty;
    /// <summary>
    /// 字典编码
    /// </summary>
    public string DictTypeCode { get; set; } = string.Empty;
    /// <summary>
    /// 字段描述
    /// </summary>
    public string ColumnComment { get; set; } = string.Empty;
    /// <summary>
    /// 主外键
    /// </summary>
    public string ColumnKey { get; set; } = string.Empty;
}