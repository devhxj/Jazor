using System;
using System.Runtime.Versioning;

namespace ECMAScript;

/// <summary>
/// 用于指定 ECMAScript 类型的名称
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, Inherited = false)]
[SupportedOSPlatform("browser")]
internal sealed class ECMAScriptNameAttribute : Attribute
{
    public ECMAScriptNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
