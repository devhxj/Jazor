using System;
using ECMAScript.VueContract.Descriptor;

namespace ECMAScript.VueContract;

/// <summary>
/// 为组件声明 Vue prop 的公开名称、绑定方式、必填性和默认值元数据。
/// </summary>
/// <remarks>
/// 这是 component contract 的编译期描述，不会生成额外的 props marker 对象。
/// RazorVue 会把它与 Razor 参数和 Vue runtime props 直接合并为最终 render-function 形状。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class VuePropAttribute : Attribute
{
    public VuePropAttribute(string publicName)
    {
        PublicName = publicName;
    }

    public VuePropAttribute(string publicName, VuePropKind kind)
        : this(publicName)
    {
        Kind = kind;
        HasKindOverride = true;
    }

    public string PublicName { get; }

    public string? Name { get; set; }

    public bool Required { get; set; }

    public bool AcceptsBinding { get; set; }

    public string? DefaultExpression { get; set; }

    public VuePropKind Kind { get; }

    internal bool HasKindOverride { get; }
}
