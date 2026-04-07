using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

[VueLibraryComponent("vuetify/components", "VTab")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VTab : VueLibraryComponent
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
