using System;
using ECMAScript.VueContract.Descriptor;

namespace ECMAScript.VueContract;

/// <summary>
/// 声明组件事件的 Razor 别名、Vue 名称、payload 类型和事件类别。
/// </summary>
/// <remarks>
/// 事件名称解析必须在编译期完成；该属性不引入中间事件对象，最终直接落到 Vue emit/handler 形状。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibraryEmitAttribute : Attribute
{
    public VueLibraryEmitAttribute(string razorAlias)
    {
        RazorAlias = razorAlias;
    }

    public VueLibraryEmitAttribute(string razorAlias, VueEmitKind kind)
        : this(razorAlias)
    {
        Kind = kind;
        HasKindOverride = true;
    }

    public string RazorAlias { get; }

    public string? Name { get; set; }

    public string? PayloadTypeName { get; set; }

    public VueEmitKind Kind { get; }

    internal bool HasKindOverride { get; }
}
