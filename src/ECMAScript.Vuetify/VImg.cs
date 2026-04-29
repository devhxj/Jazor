using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify image stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VImg")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VImg : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Src { get; set; }

    [Parameter]
    public string? Alt { get; set; }

    [Parameter]
    public string? Height { get; set; }

    [Parameter]
    public string? Width { get; set; }

    [Parameter]
    public bool Cover { get; set; }
}
