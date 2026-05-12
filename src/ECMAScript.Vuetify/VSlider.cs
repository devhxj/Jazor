using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 滑块组件的编写代理。
/// Vuetify slider authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSlider")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
public sealed class VSlider : ComponentBase, IVueLibraryComponent
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
    public Number? ModelValue { get; set; }

    [Parameter]
    public EventCallback<Number?> ModelValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
