namespace Jero.Core.Service;

/// <summary>
/// 数据库表
/// </summary>
public class TableOutput
{
    /// <summary>
    /// 库定位器名
    /// </summary>
    public string ConfigId { get; set; } = string.Empty;
    /// <summary>
    /// 表名（字母形式的）
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// 实体名称
    /// </summary>
    public string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// 创建时间
    /// </summary>
    public string CreateTime { get; set; } = string.Empty;
    /// <summary>
    /// 更新时间
    /// </summary>
    public string UpdateTime { get; set; } = string.Empty;
    /// <summary>
    /// 表名称描述（功能名）
    /// </summary>
    public string TableComment { get; set; } = string.Empty;
}