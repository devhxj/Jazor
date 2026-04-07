using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

[VueLibraryComponent("vuetify/components", "VSelect")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VSelect : VueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }
}
