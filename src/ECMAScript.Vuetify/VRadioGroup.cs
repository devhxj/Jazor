using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VRadioGroup")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VRadioGroup : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    [Parameter]
    public VuetifyMessagesValue? Messages { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Inline { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
