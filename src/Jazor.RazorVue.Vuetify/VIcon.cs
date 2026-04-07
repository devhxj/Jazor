using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

/// <summary>
/// First-wave Vuetify icon stub with a minimal prop surface.
/// </summary>
[VueLibraryComponent("vuetify/components", "VIcon")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VIcon : VueLibraryComponent
{
    [Parameter]
    public string? Icon { get; set; }
}
