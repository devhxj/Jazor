using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify sparkline authoring proxy for compact trend and bar visualizations.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSparkline")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Label), Name = "label")]
public sealed class VSparkline : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool AutoDraw { get; set; }

    [Parameter]
    public VueStringNumberValue? AutoDrawDuration { get; set; }

    [Parameter]
    public string? AutoDrawEasing { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string[]? Gradient { get; set; }

    [Parameter]
    public VuetifySparklineGradientDirection? GradientDirection { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VuetifySparklineItems? Labels { get; set; }

    [Parameter]
    public VueStringNumberValue? LabelSize { get; set; }

    [Parameter]
    public VueStringNumberValue? LineWidth { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? ItemValue { get; set; }

    [Parameter]
    public VuetifySparklineItems? ModelValue { get; set; }

    [Parameter]
    public VueStringNumberValue? Min { get; set; }

    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    [Parameter]
    public VueStringNumberValue? Padding { get; set; }

    [Parameter]
    public bool ShowLabels { get; set; }

    [Parameter]
    public VuetifySparklineSmoothValue? Smooth { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public bool Fill { get; set; }

    [Parameter]
    public bool AutoLineWidth { get; set; }

    [Parameter]
    public VuetifySparklineType? Type { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VSparklineLabelSlotContext>? Label { get; set; }
}
