using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

[VueLibraryComponent("vuetify/components", "VDivider")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VDivider : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Inset { get; set; }

    [Parameter]
    public int? Thickness { get; set; }

    [Parameter]
    public bool Vertical { get; set; }
}
