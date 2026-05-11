using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VNumberInput")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
public sealed class VNumberInput : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Inset { get; set; }

    [Parameter]
    public bool Reverse { get; set; }

    [Parameter]
    public bool HideInput { get; set; }

    [Parameter]
    public VuetifyNumberInputControlVariant? ControlVariant { get; set; }

    [Parameter]
    public Number? Min { get; set; }

    [Parameter]
    public Number? Max { get; set; }

    [Parameter]
    public Number? Step { get; set; }

    [Parameter]
    public Number? Precision { get; set; }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VuetifyFieldVariant? Variant { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? Hint { get; set; }

    [Parameter]
    public bool PersistentHint { get; set; }

    [Parameter]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    [Parameter]
    public VuetifyMessagesValue? Messages { get; set; }

    [Parameter]
    public Number? ModelValue { get; set; }

    [Parameter]
    public EventCallback<Number?> ModelValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
