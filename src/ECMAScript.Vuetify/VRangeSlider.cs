using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 范围滑块组件。
/// Vuetify range slider component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VRangeSlider")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
public sealed class VRangeSlider : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? TrackColor { get; set; }

    [Parameter]
    public string? ThumbColor { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public VuetifyBooleanAlwaysValue? ThumbLabel { get; set; }

    [Parameter]
    public VuetifyBooleanAlwaysValue? ShowTicks { get; set; }

    [Parameter]
    public Number? Min { get; set; }

    [Parameter]
    public Number? Max { get; set; }

    [Parameter]
    public Number? Step { get; set; }

    [Parameter]
    public bool Strict { get; set; }

    [Parameter]
    public VuetifySliderDirection? Direction { get; set; }

    [Parameter]
    public VuetifyRangeSliderModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyRangeSliderModelValue?> ModelValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
