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
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VTimeline : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    [Parameter]
    public string? IconColor { get; set; }

    [Parameter]
    public string? DotColor { get; set; }

    [Parameter]
    public bool FillDot { get; set; }

    [Parameter]
    public bool? HideOpposite { get; set; }

    [Parameter]
    public VueStringNumberValue? LineInset { get; set; }

    [Parameter]
    public VuetifyTimelineAlign? Align { get; set; }

    [Parameter]
    public VuetifyTimelineDirection? Direction { get; set; }

    [Parameter]
    public VuetifyTimelineJustify? Justify { get; set; }

    [Parameter]
    public VuetifyTimelineSide? Side { get; set; }

    [Parameter]
    public VueStringNumberValue? LineThickness { get; set; }

    [Parameter]
    public string? LineColor { get; set; }

    [Parameter]
    public VuetifyTimelineTruncateLine? TruncateLine { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
