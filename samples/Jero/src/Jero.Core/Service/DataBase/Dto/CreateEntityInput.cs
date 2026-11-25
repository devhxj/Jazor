namespace Jero.Core.Service;

public class CreateEntityInput
{
    /// <summary>
    /// 表名
    /// </summary>
    /// <example>student</example>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    /// 实体名
    /// </summary>
    /// <example>Student</example>
    public string EntityName { get; set; } = string.Empty;
    /// <summary>
    /// 基类名
    /// </summary>
    /// <example>AutoIncrementEntity</example>
    public string BaseClassName { get; set; } = string.Empty;
    /// <summary>
    /// 导出位置
    /// </summary>
    /// <example>Web.Application</example>
    public string Position { get; set; } = string.Empty;
    /// <summary>
    /// 库标识
    /// </summary>
    public string ConfigId { get; set; } = string.Empty;
}