using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

[VueLibraryComponent("vuetify/components", "VCol")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VCol : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public int? Cols { get; set; }

    [Parameter]
    public int? Md { get; set; }

    [Parameter]
    public int? Lg { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
