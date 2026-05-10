using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VFileInput")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VFileInput : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Accept { get; set; }

    [Parameter]
    public bool Chips { get; set; }

    [Parameter]
    public bool Counter { get; set; }

    [Parameter]
    public VuetifyFileShowSizeValue? ShowSize { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

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
    public VuetifyFileModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyFileModelValue?> ModelValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
