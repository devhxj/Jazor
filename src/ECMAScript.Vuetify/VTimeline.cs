using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 时间线组件的编写代理，用于按时间顺序排列的垂直或水平内容。
/// Vuetify timeline authoring proxy for chronological vertical or horizontal content.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTimeline")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class VTimeline : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 紧凑度。
    /// Density.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 尺寸。
    /// Size.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 图标颜色。
    /// Icon color.
    /// </summary>
    [Parameter]
    public string? IconColor { get; set; }

    /// <summary>
    /// 圆点颜色。
    /// Dot color.
    /// </summary>
    [Parameter]
    public string? DotColor { get; set; }

    /// <summary>
    /// 填充圆点。
    /// Fills the dot.
    /// </summary>
    [Parameter]
    public bool FillDot { get; set; }

    /// <summary>
    /// 隐藏对侧内容。
    /// Hides the opposite side content.
    /// </summary>
    [Parameter]
    public bool? HideOpposite { get; set; }

    /// <summary>
    /// 线条内缩。
    /// Line inset.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? LineInset { get; set; }

    /// <summary>
    /// 对齐方式。
    /// Alignment.
    /// </summary>
    [Parameter]
    public VuetifyTimelineAlign? Align { get; set; }

    /// <summary>
    /// 方向。
    /// Direction.
    /// </summary>
    [Parameter]
    public VuetifyTimelineDirection? Direction { get; set; }

    /// <summary>
    /// 对齐分布。
    /// Justification.
    /// </summary>
    [Parameter]
    public VuetifyTimelineJustify? Justify { get; set; }

    /// <summary>
    /// 侧边。
    /// Side.
    /// </summary>
    [Parameter]
    public VuetifyTimelineSide? Side { get; set; }

    /// <summary>
    /// 线条粗细。
    /// Line thickness.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? LineThickness { get; set; }

    /// <summary>
    /// 线条颜色。
    /// Line color.
    /// </summary>
    [Parameter]
    public string? LineColor { get; set; }

    /// <summary>
    /// 截断线条。
    /// Truncates the line.
    /// </summary>
    [Parameter]
    public VuetifyTimelineTruncateLine? TruncateLine { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
