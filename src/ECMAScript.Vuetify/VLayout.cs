using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 布局组件，用于管理页面区域的布局结构。
/// Vuetify layout component for managing page region layout structure.
/// </summary>
[VueLibraryComponent("vuetify/components", "VLayout")]
public sealed class VLayout : ComponentBase
{
    /// <summary>
    /// 允许重叠的区域名称数组。
    /// Array of region names that are allowed to overlap.
    /// </summary>
    [Parameter]
    public string[]? Overlaps { get; set; }

    /// <summary>
    /// 是否占满全高。
    /// Whether to take up full height.
    /// </summary>
    [Parameter]
    public bool FullHeight { get; set; }

    /// <summary>
    /// 布局的高度。
    /// Height of the layout.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 布局的最大高度。
    /// Maximum height of the layout.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 布局的最大宽度。
    /// Maximum width of the layout.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 布局的最小高度。
    /// Minimum height of the layout.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 布局的最小宽度。
    /// Minimum width of the layout.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 布局的宽度。
    /// Width of the layout.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
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
