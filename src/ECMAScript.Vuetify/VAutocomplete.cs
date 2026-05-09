using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VAutocomplete")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VAutocomplete : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public IEnumerable<object>? Items { get; set; }

    [Parameter]
    public string? ItemTitle { get; set; }

    [Parameter]
    public string? ItemValue { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public bool Chips { get; set; }

    [Parameter]
    public bool ReturnObject { get; set; }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public VueDictionary? MenuProps { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public string? NoDataText { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
