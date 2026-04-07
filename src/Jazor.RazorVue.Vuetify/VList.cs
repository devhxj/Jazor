using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

[VueLibraryComponent("vuetify/components", "VList")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VList : VueLibraryComponent
{
    [Parameter]
    public string? Density { get; set; }

    [Parameter]
    public bool Nav { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
