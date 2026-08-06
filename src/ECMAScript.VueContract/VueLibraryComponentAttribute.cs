using System;

namespace ECMAScript.VueContract;

/// <summary>
/// 声明外部 Vue 组件的 JavaScript named import。
/// </summary>
/// <remarks>
/// 该属性只提供静态绑定元数据；实际 import 收集和模块头发射由 RazorVue 层负责。
/// 库的 CSS 资源由随 NuGet 包交付的 <c>manifest.json</c> 统一声明和物化，
/// 不在组件上重复声明浏览器 URL。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VueLibraryComponentAttribute(string importSpecifier, string exportName) : Attribute
{
    public string ImportSpecifier { get; } = importSpecifier;

    public string ExportName { get; } = exportName;
}
