using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VSwitch")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VSwitch : ComponentBase, IVueLibraryComponent
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
    public bool Inset { get; set; }

    [Parameter]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
