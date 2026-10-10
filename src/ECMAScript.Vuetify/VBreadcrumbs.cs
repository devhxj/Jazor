using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// 第一波 Vuetify 面包屑导航存根，用于 RazorVue 创作。
/// First-wave Vuetify breadcrumbs stub for RazorVue authoring.
/// </summary>
[ECMAScript("vuetify/components/VBreadcrumbs")]
public sealed class VBreadcrumbs : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 面包屑导航项列表。
    /// Breadcrumb navigation items.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBreadcrumbItems?；值域为 VuetifyBreadcrumbItemValue[]。变量用 Items="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("items")]
    public VuetifyBreadcrumbItems? Items { get; set; }

    /// <summary>
    /// 项之间的分隔符。
    /// Divider between items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("divider")]
    public string? Divider { get; set; }

    /// <summary>
    /// 是否禁用组件。
    /// Disables the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
