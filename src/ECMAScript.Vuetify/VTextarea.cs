using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VTextarea")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VTextarea : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? Hint { get; set; }

    [Parameter]
    public bool PersistentHint { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public bool AutoGrow { get; set; }

    [Parameter]
    public VueIntStringValue? Counter { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public int? Rows { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
