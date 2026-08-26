using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 时间线组件的编写代理，用于按时间顺序排列的垂直或水平内容。
/// Vuetify timeline authoring proxy for chronological vertical or horizontal content.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VTimeline")]
public sealed class VTimeline : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 紧凑度。
    /// Density.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 尺寸。
    /// Size.
    /// </summary>
    [Parameter]
    [ECMAScriptName("size")]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 图标颜色。
    /// Icon color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("iconColor")]
    public string? IconColor { get; set; }

    /// <summary>
    /// 圆点颜色。
    /// Dot color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("dotColor")]
    public string? DotColor { get; set; }

    /// <summary>
    /// 填充圆点。
    /// Fills the dot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fillDot")]
    public bool FillDot { get; set; }

    /// <summary>
    /// 隐藏对侧内容。
    /// Hides the opposite side content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideOpposite")]
    public bool? HideOpposite { get; set; }

    /// <summary>
    /// 线条内缩。
    /// Line inset.
    /// </summary>
    [Parameter]
    [ECMAScriptName("lineInset")]
    public VueStringNumberValue? LineInset { get; set; }

    /// <summary>
    /// 对齐方式。
    /// Alignment.
    /// </summary>
    [Parameter]
    [ECMAScriptName("align")]
    public VuetifyTimelineAlign? Align { get; set; }

    /// <summary>
    /// 方向。
    /// Direction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("direction")]
    public VuetifyTimelineDirection? Direction { get; set; }

    /// <summary>
    /// 对齐分布。
    /// Justification.
    /// </summary>
    [Parameter]
    [ECMAScriptName("justify")]
    public VuetifyTimelineJustify? Justify { get; set; }

    /// <summary>
    /// 侧边。
    /// Side.
    /// </summary>
    [Parameter]
    [ECMAScriptName("side")]
    public VuetifyTimelineSide? Side { get; set; }

    /// <summary>
    /// 线条粗细。
    /// Line thickness.
    /// </summary>
    [Parameter]
    [ECMAScriptName("lineThickness")]
    public VueStringNumberValue? LineThickness { get; set; }

    /// <summary>
    /// 线条颜色。
    /// Line color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("lineColor")]
    public string? LineColor { get; set; }

    /// <summary>
    /// 截断线条。
    /// Truncates the line.
    /// </summary>
    [Parameter]
    [ECMAScriptName("truncateLine")]
    public VuetifyTimelineTruncateLine? TruncateLine { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
