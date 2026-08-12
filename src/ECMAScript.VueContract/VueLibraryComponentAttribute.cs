using System;
using ECMAScript.Contract;

namespace ECMAScript.VueContract;

/// <summary>
/// Declares the Vue-specific form of an external library component named import.
/// 声明外部 Vue 组件 named import 的 Vue 专属形式。
/// </summary>
/// <remarks>
/// This compatibility-facing attribute derives from the framework-neutral
/// <see cref="LibraryComponentAttribute"/> contract. Shared analysis recognizes the
/// neutral base contract, while RazorVue owns the Vue render-function interpretation.
/// <para>
/// 本特性继承框架中性的 <see cref="LibraryComponentAttribute"/> 契约。共享分析器识别
/// 中性基契约，而 RazorVue 负责 Vue render-function 的具体解释。
/// </para>
/// 该属性只提供静态绑定元数据；实际 import 收集和模块头发射由 RazorVue 层负责。
/// 库的 CSS 资源由随 NuGet 包交付的 <c>manifest.json</c> 统一声明和物化，
/// 不在组件上重复声明浏览器 URL。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VueLibraryComponentAttribute(string importSpecifier, string exportName)
    : LibraryComponentAttribute(importSpecifier, exportName)
{
    /// <inheritdoc />
    public override string ImportSpecifier => base.ImportSpecifier;

    /// <inheritdoc />
    public override string ExportName => base.ExportName;
}
