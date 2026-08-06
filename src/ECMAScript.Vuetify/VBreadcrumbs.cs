using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// 第一波 Vuetify 面包屑导航存根，用于 RazorVue 创作。
/// First-wave Vuetify breadcrumbs stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VBreadcrumbs")]
public sealed class VBreadcrumbs : ComponentBase
{
    /// <summary>
    /// 面包屑导航项列表。
    /// Breadcrumb navigation items.
    /// </summary>
    [Parameter]
    public VuetifyBreadcrumbItems? Items { get; set; }

    /// <summary>
    /// 项之间的分隔符。
    /// Divider between items.
    /// </summary>
    [Parameter]
    public string? Divider { get; set; }

    /// <summary>
    /// 是否禁用组件。
    /// Disables the component.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
