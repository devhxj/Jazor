using System;

namespace ECMAScript;

/// <summary>
/// 用于指定 ECMAScript 符号的运行时名称。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class ECMAScriptNameAttribute : Attribute
{
    public ECMAScriptNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
