using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VCardText")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VCardText : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
