namespace ECMAScript.Contract;

/// <summary>
/// Declares the ECMAScript named import that supplies an external library component.
/// 声明提供外部库组件的 ECMAScript named import。
/// </summary>
/// <remarks>
/// <para>
/// This is discovery metadata, not a renderer protocol. It records only the package
/// specifier and exported component name, so the shared analyzer can recognize a
/// component wrapper without taking a dependency on Vue, React, or another framework.
/// </para>
/// <para>
/// 该特性仅描述发现元数据，不定义渲染协议。它只记录包 specifier 与导出组件名，
/// 使共享分析器能够识别组件包装类型，而无需依赖 Vue、React 或其他框架。
/// </para>
/// <para>
/// A framework adapter may apply this attribute directly or expose a framework-specific
/// derived attribute. The adapter that owns rendering remains responsible for interpreting
/// the import and its component protocol; the core must not infer one framework's behavior
/// from this neutral declaration.
/// </para>
/// <para>
/// 框架适配层可以直接使用本特性，也可以公开框架专属的派生特性。拥有渲染职责的适配层
/// 仍负责解释该 import 及其组件协议；核心不得从这个中性声明推断任一框架的行为。
/// </para>
/// </remarks>
/// <param name="importSpecifier">The ECMAScript module specifier that exports the component. 导出组件的 ECMAScript 模块 specifier。</param>
/// <param name="exportName">The named export that identifies the component. 标识组件的 named export。</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class LibraryComponentAttribute(string importSpecifier, string exportName) : Attribute
{
    /// <summary>
    /// Gets the ECMAScript module specifier that exports the component.
    /// 获取导出该组件的 ECMAScript 模块 specifier。
    /// </summary>
    public virtual string ImportSpecifier { get; } = importSpecifier;

    /// <summary>
    /// Gets the named ECMAScript export that identifies the component.
    /// 获取标识该组件的 ECMAScript named export。
    /// </summary>
    public virtual string ExportName { get; } = exportName;
}
