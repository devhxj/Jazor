using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify image stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VImg")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VImg : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Src { get; set; }

    [Parameter]
    public string? Alt { get; set; }

    [Parameter]
    public string? LazySrc { get; set; }

    [Parameter]
    public string? Srcset { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VueStringNumberValue? AspectRatio { get; set; }

    [Parameter]
    public bool Cover { get; set; }

    [Parameter]
    public bool Eager { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
