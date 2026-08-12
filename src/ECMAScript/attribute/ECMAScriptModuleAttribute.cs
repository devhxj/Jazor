using System.Runtime.Versioning;

namespace ECMAScript;

/// <summary>
/// Marks a type whose declaration is emitted as an ECMAScript module artifact.
/// 标记其声明会作为 ECMAScript 模块产物发出的类型。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[SupportedOSPlatform("browser")]
public sealed class ECMAScriptModuleAttribute : Attribute
{
    /// <summary>
    /// Gets the output module specifier represented by the marked type.
    /// 获取被标记类型所表示的输出模块 specifier。
    /// </summary>
    public string? Export { get; }

    public ECMAScriptModuleAttribute()
    {
        Export = null;
    }

    /// <summary>
    /// Marks a type that is emitted into the supplied ECMAScript module specifier.
    /// 标记类型会被发射到指定的 ECMAScript 模块 specifier。
    /// </summary>
    /// <param name="export">Output module specifier. 输出模块 specifier。</param>
    public ECMAScriptModuleAttribute(string export)
    {
        Export = export;
    }
}
