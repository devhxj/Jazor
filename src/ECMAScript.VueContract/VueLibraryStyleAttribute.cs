using System;

namespace ECMAScript.VueContract;

/// <summary>声明外部组件依赖的样式模块或样式 specifier。</summary>
/// <remarks>样式依赖属于组件元数据，具体导入和 materialization 由 emit 层处理。</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class VueLibraryStyleAttribute(string styleSpecifier) : Attribute
{
    public string StyleSpecifier { get; } = styleSpecifier;
}
