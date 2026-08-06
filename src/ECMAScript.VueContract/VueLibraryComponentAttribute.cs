using System;

namespace ECMAScript.VueContract;

/// <summary>
/// 声明外部 Vue 组件的 JavaScript 导入和官方样式依赖。
/// </summary>
/// <remarks>
/// 该属性只提供静态绑定元数据；实际 import 收集和模块头发射由 RazorVue 层负责。
/// <see cref="StyleUrls"/> 表示浏览器 stylesheet URL，而不是 ECMAScript CSS module import。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VueLibraryComponentAttribute(string importSpecifier, string exportName) : Attribute
{
    public string ImportSpecifier { get; } = importSpecifier;

    public string ExportName { get; } = exportName;

    /// <summary>
    /// Gets or sets the official browser stylesheets required by this component library.
    /// </summary>
    /// <remarks>
    /// RazorVue gathers these URLs from the components actually used by an artifact, then loads
    /// them through its browser runtime. Keep values as absolute or app-resolvable stylesheet URLs.
    /// </remarks>
    public string[] StyleUrls { get; set; } = [];
}
