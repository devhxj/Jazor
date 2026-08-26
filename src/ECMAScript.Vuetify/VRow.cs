using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 网格行组件的编写代理。
/// Vuetify grid row authoring proxy.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VRow")]
public sealed class VRow : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 渲染根元素时使用的 HTML 标签。
    /// The HTML tag used for the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 应用于根元素的 CSS 类。
    /// CSS classes applied to the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用于根元素的内联样式。
    /// Inline styles applied to the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 小屏幕下的多行对齐方式。
    /// Multi-row alignment on small screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignContentSm")]
    public string? AlignContentSm { get; set; }

    /// <summary>
    /// 中等屏幕下的多行对齐方式。
    /// Multi-row alignment on medium screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignContentMd")]
    public string? AlignContentMd { get; set; }

    /// <summary>
    /// 大屏幕下的多行对齐方式。
    /// Multi-row alignment on large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignContentLg")]
    public string? AlignContentLg { get; set; }

    /// <summary>
    /// 超大屏幕下的多行对齐方式。
    /// Multi-row alignment on extra-large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignContentXl")]
    public string? AlignContentXl { get; set; }

    /// <summary>
    /// 超超大屏幕下的多行对齐方式。
    /// Multi-row alignment on XXL screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignContentXxl")]
    public string? AlignContentXxl { get; set; }

    /// <summary>
    /// 多行内容的垂直对齐方式。
    /// The vertical alignment of multi-row content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignContent")]
    public string? AlignContent { get; set; }

    /// <summary>
    /// 小屏幕下的水平排列方式。
    /// Horizontal justification on small screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("justifySm")]
    public string? JustifySm { get; set; }

    /// <summary>
    /// 中等屏幕下的水平排列方式。
    /// Horizontal justification on medium screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("justifyMd")]
    public string? JustifyMd { get; set; }

    /// <summary>
    /// 大屏幕下的水平排列方式。
    /// Horizontal justification on large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("justifyLg")]
    public string? JustifyLg { get; set; }

    /// <summary>
    /// 超大屏幕下的水平排列方式。
    /// Horizontal justification on extra-large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("justifyXl")]
    public string? JustifyXl { get; set; }

    /// <summary>
    /// 超超大屏幕下的水平排列方式。
    /// Horizontal justification on XXL screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("justifyXxl")]
    public string? JustifyXxl { get; set; }

    /// <summary>
    /// 水平排列方式。
    /// The horizontal justification of columns.
    /// </summary>
    [Parameter]
    [ECMAScriptName("justify")]
    public string? Justify { get; set; }

    /// <summary>
    /// 小屏幕下的垂直对齐方式。
    /// Vertical alignment on small screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignSm")]
    public string? AlignSm { get; set; }

    /// <summary>
    /// 中等屏幕下的垂直对齐方式。
    /// Vertical alignment on medium screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignMd")]
    public string? AlignMd { get; set; }

    /// <summary>
    /// 大屏幕下的垂直对齐方式。
    /// Vertical alignment on large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignLg")]
    public string? AlignLg { get; set; }

    /// <summary>
    /// 超大屏幕下的垂直对齐方式。
    /// Vertical alignment on extra-large screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignXl")]
    public string? AlignXl { get; set; }

    /// <summary>
    /// 超超大屏幕下的垂直对齐方式。
    /// Vertical alignment on XXL screens.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alignXxl")]
    public string? AlignXxl { get; set; }

    /// <summary>
    /// 是否减小行内列之间的间距。
    /// Whether to reduce the spacing between columns.
    /// </summary>
    [Parameter]
    [ECMAScriptName("dense")]
    public bool Dense { get; set; }

    /// <summary>
    /// 是否移除列间距。
    /// Whether to remove the gutter spacing between columns.
    /// </summary>
    [Parameter]
    [ECMAScriptName("noGutters")]
    public bool NoGutters { get; set; }

    /// <summary>
    /// 垂直对齐方式。
    /// The vertical alignment of columns.
    /// </summary>
    [Parameter]
    [ECMAScriptName("align")]
    public string? Align { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容，用于放置列组件。
    /// The default slot for placing column components.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
