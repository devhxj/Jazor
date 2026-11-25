namespace Jero.Core;

/// <summary>
/// 代码生成配置选项
/// </summary>
public sealed class CodeGenOptions : IConfigurableOptions
{
    /// <summary>
    /// 数据库实体程序集名称集合
    /// </summary>
    public List<string> EntityAssemblyNames { get; set; } = [];
    /// <summary>
    /// 数据库实体基类名称集合
    /// </summary>
    public List<string> BaseEntityNames { get; set; } = [];
    /// <summary>
    /// 后端生成到的项目
    /// </summary>
    public List<string> BackendApplicationNamespaces { get; set; } = [];
}
