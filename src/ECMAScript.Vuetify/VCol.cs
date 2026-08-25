using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 网格列组件创作代理。
/// Vuetify grid column component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCol")]
public sealed class VCol : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
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
    [ECMAScriptName("alignSelf")]
    public string? AlignSelf { get; set; }

    /// <summary>
    /// 小型屏幕上的排序顺序。
    /// The sort order on small screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("orderSm")]
    public VueStringNumberValue? OrderSm { get; set; }

    /// <summary>
    /// 中型屏幕上的排序顺序。
    /// The sort order on medium screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("orderMd")]
    public VueStringNumberValue? OrderMd { get; set; }

    /// <summary>
    /// 大型屏幕上的排序顺序。
    /// The sort order on large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("orderLg")]
    public VueStringNumberValue? OrderLg { get; set; }

    /// <summary>
    /// 超大型屏幕上的排序顺序。
    /// The sort order on extra-large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("orderXl")]
    public VueStringNumberValue? OrderXl { get; set; }

    /// <summary>
    /// 超超大型屏幕上的排序顺序。
    /// The sort order on extra-extra-large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("orderXxl")]
    public VueStringNumberValue? OrderXxl { get; set; }

    /// <summary>
    /// 默认排序顺序。
    /// The default sort order.
    /// </summary>
    [Parameter]
    [ECMAScriptName("order")]
    public VueStringNumberValue? Order { get; set; }

    /// <summary>
    /// 小型屏幕上的列偏移量。
    /// The column offset on small screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offsetSm")]
    public VueStringNumberValue? OffsetSm { get; set; }

    /// <summary>
    /// 中型屏幕上的列偏移量。
    /// The column offset on medium screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offsetMd")]
    public VueStringNumberValue? OffsetMd { get; set; }

    /// <summary>
    /// 大型屏幕上的列偏移量。
    /// The column offset on large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offsetLg")]
    public VueStringNumberValue? OffsetLg { get; set; }

    /// <summary>
    /// 超大型屏幕上的列偏移量。
    /// The column offset on extra-large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offsetXl")]
    public VueStringNumberValue? OffsetXl { get; set; }

    /// <summary>
    /// 超超大型屏幕上的列偏移量。
    /// The column offset on extra-extra-large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offsetXxl")]
    public VueStringNumberValue? OffsetXxl { get; set; }

    /// <summary>
    /// 默认列偏移量。
    /// The default column offset.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offset")]
    public VueStringNumberValue? Offset { get; set; }

    /// <summary>
    /// 小型屏幕上的列跨度。
    /// The column span on small screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("sm")]
    public VuetifyGridSpanValue? Sm { get; set; }

    /// <summary>
    /// 中型屏幕上的列跨度。
    /// The column span on medium screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("md")]
    public VuetifyGridSpanValue? Md { get; set; }

    /// <summary>
    /// 大型屏幕上的列跨度。
    /// The column span on large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("lg")]
    public VuetifyGridSpanValue? Lg { get; set; }

    /// <summary>
    /// 超大型屏幕上的列跨度。
    /// The column span on extra-large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("xl")]
    public VuetifyGridSpanValue? Xl { get; set; }

    /// <summary>
    /// 超超大型屏幕上的列跨度。
    /// The column span on extra-extra-large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("xxl")]
    public VuetifyGridSpanValue? Xxl { get; set; }

    /// <summary>
    /// 默认列跨度。
    /// The default column span.
    /// </summary>
    [Parameter]
    [ECMAScriptName("cols")]
    public VuetifyGridSpanValue? Cols { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Slot for child content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
