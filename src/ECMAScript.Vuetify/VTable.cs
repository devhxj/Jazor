using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 表格组件的编写代理。
/// Vuetify table authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTable", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
public sealed class VTable : ComponentBase
{
    /// <summary>
    /// 是否固定表头。
    /// Whether to fix the table header.
    /// </summary>
    [Parameter]
    public bool FixedHeader { get; set; }

    /// <summary>
    /// 是否固定表尾。
    /// Whether to fix the table footer.
    /// </summary>
    [Parameter]
    public bool FixedFooter { get; set; }

    /// <summary>
    /// 组件的高度。
    /// Height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 是否在悬停行时高亮显示。
    /// Whether to highlight rows on hover.
    /// </summary>
    [Parameter]
    public bool Hover { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// Component density level.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 渲染的根 HTML 元素标签名。
    /// Root HTML element tag name.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

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
