using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 网格列组件创作代理。
/// Vuetify grid column component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCol")]
public sealed class VCol : ComponentBase
{
    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 应用的 CSS 类。
    /// The CSS class to apply.
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用的内联样式。
    /// The inline style to apply.
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 列的自身垂直对齐方式。
    /// The vertical alignment of the column itself.
    /// </summary>
    [Parameter]
    public string? AlignSelf { get; set; }

    /// <summary>
    /// 小型屏幕上的排序顺序。
    /// The sort order on small screens.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OrderSm { get; set; }

    /// <summary>
    /// 中型屏幕上的排序顺序。
    /// The sort order on medium screens.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OrderMd { get; set; }

    /// <summary>
    /// 大型屏幕上的排序顺序。
    /// The sort order on large screens.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OrderLg { get; set; }

    /// <summary>
    /// 超大型屏幕上的排序顺序。
    /// The sort order on extra-large screens.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OrderXl { get; set; }

    /// <summary>
    /// 超超大型屏幕上的排序顺序。
    /// The sort order on extra-extra-large screens.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OrderXxl { get; set; }

    /// <summary>
    /// 默认排序顺序。
    /// The default sort order.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Order { get; set; }

    /// <summary>
    /// 小型屏幕上的列偏移量。
    /// The column offset on small screens.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OffsetSm { get; set; }

    /// <summary>
    /// 中型屏幕上的列偏移量。
    /// The column offset on medium screens.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OffsetMd { get; set; }

    /// <summary>
    /// 大型屏幕上的列偏移量。
    /// The column offset on large screens.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OffsetLg { get; set; }

    /// <summary>
    /// 超大型屏幕上的列偏移量。
    /// The column offset on extra-large screens.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OffsetXl { get; set; }

    /// <summary>
    /// 超超大型屏幕上的列偏移量。
    /// The column offset on extra-extra-large screens.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OffsetXxl { get; set; }

    /// <summary>
    /// 默认列偏移量。
    /// The default column offset.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Offset { get; set; }

    /// <summary>
    /// 小型屏幕上的列跨度。
    /// The column span on small screens.
    /// </summary>
    [Parameter]
    public VuetifyGridSpanValue? Sm { get; set; }

    /// <summary>
    /// 中型屏幕上的列跨度。
    /// The column span on medium screens.
    /// </summary>
    [Parameter]
    public VuetifyGridSpanValue? Md { get; set; }

    /// <summary>
    /// 大型屏幕上的列跨度。
    /// The column span on large screens.
    /// </summary>
    [Parameter]
    public VuetifyGridSpanValue? Lg { get; set; }

    /// <summary>
    /// 超大型屏幕上的列跨度。
    /// The column span on extra-large screens.
    /// </summary>
    [Parameter]
    public VuetifyGridSpanValue? Xl { get; set; }

    /// <summary>
    /// 超超大型屏幕上的列跨度。
    /// The column span on extra-extra-large screens.
    /// </summary>
    [Parameter]
    public VuetifyGridSpanValue? Xxl { get; set; }

    /// <summary>
    /// 默认列跨度。
    /// The default column span.
    /// </summary>
    [Parameter]
    public VuetifyGridSpanValue? Cols { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Slot for child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
