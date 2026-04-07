using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

[VueLibraryComponent("vuetify/components", "VForm")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VForm : VueLibraryComponent
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool FastFail { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
