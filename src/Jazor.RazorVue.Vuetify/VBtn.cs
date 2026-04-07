using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

/// <summary>
/// First-wave Vuetify button stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VBtn")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VBtn : VueLibraryComponent
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
