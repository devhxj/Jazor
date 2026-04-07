using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

[VueLibraryComponent("vuetify/components", "VRow")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VRow : VueLibraryComponent
{
    [Parameter]
    public string? Align { get; set; }

    [Parameter]
    public string? Justify { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
