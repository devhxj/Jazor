using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

/// <summary>
/// First scoped-slot Vuetify example for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDialog")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VDialog : VueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public RenderFragment<VDialogActivatorContext>? Activator { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
