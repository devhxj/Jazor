namespace Jero.Core;

/// <summary>
/// 属性字典配置
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ImportDictAttribute : Attribute
{
    /// <summary>
    /// 字典Code
    /// </summary>
    public string TypeCode { get; set; } = string.Empty;
    /// <summary>
    /// 目标对象名称
    /// </summary>
    public string TargetPropName { get; set; } = string.Empty;
}