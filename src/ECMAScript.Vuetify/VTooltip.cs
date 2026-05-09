using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify tooltip stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTooltip")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VTooltip : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public bool OpenOnHover { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? Activator { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
