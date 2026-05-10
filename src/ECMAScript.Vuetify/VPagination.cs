using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify pagination stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VPagination")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VPagination : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public int ModelValue { get; set; }

    [Parameter]
    public EventCallback<int> ModelValueChanged { get; set; }

    [Parameter]
    public VueStringNumberValue? Length { get; set; }

    [Parameter]
    public VueStringNumberValue? TotalVisible { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
