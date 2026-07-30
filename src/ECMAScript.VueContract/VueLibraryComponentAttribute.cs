using System;

namespace ECMAScript.VueContract;

/// <summary>
/// 声明外部 Vue 组件的 import specifier 和导出名称。
/// </summary>
/// <remarks>该属性只提供静态绑定元数据；实际 import 收集和模块头发射由 compiler/emit 层负责。</remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VueLibraryComponentAttribute(string importSpecifier, string exportName) : Attribute
{
    public string ImportSpecifier { get; } = importSpecifier;

    public string ExportName { get; } = exportName;
}
