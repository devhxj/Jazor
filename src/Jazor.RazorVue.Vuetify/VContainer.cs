using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

[VueLibraryComponent("vuetify/components", "VContainer")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VContainer : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Fluid { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
