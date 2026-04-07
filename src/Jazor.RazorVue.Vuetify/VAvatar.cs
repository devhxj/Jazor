using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

[VueLibraryComponent("vuetify/components", "VAvatar")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VAvatar : VueLibraryComponent
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? Image { get; set; }

    [Parameter]
    public string? Size { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
