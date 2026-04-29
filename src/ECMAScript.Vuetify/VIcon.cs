using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify icon stub with a minimal prop surface.
/// </summary>
[VueLibraryComponent("vuetify/components", "VIcon")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VIcon : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Icon { get; set; }
}
