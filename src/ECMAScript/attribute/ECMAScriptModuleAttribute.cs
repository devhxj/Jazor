using System.Runtime.Versioning;

namespace ECMAScript;

/// <summary>
/// 用于标记类使用 ECMAScript 语法校验
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[SupportedOSPlatform("browser")]
public sealed class ECMAScriptModuleAttribute : Attribute
{
    /// <summary>
    /// 目标类映射的 ECMAScript module 文件路径（就是最终生成到什么路径）
    /// </summary>
    public string? Export { get; }

    public ECMAScriptModuleAttribute()
    {
        Export = null;
    }

    /// <summary>
    /// 指示该类是否为原生 ECMAScript 语法
    /// </summary>
    /// <param name="export">该类映射的 ECMAScript module 文件路径</param>
    public ECMAScriptModuleAttribute(string export)
    {
        Export = export;
    }
}
