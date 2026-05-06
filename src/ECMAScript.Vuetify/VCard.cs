using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify card stub for child-content composition.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCard")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VCard : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
