namespace Jero.Core.Service;

/// <summary>
/// 枚举类型输出参数
/// </summary>
public class EnumTypeOutput
{
    /// <summary>
    /// 枚举类型描述
    /// </summary>
    public string TypeDescribe { get; set; } = string.Empty;
    /// <summary>
    /// 枚举类型名称
    /// </summary>
    public string TypeName { get; set; } = string.Empty;
    /// <summary>
    /// 枚举类型全名称
    /// </summary>
    public string TypeFullName { get; set; } = string.Empty;
    /// <summary>
    /// 枚举类型备注
    /// </summary>
    public string TypeRemark { get; set; } = string.Empty;
    /// <summary>
    /// 枚举实体
    /// </summary>
    public List<EnumEntity> EnumEntities { get; set; } = [];
}