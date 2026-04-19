using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

/// <summary>
/// First-wave Vuetify pagination stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VPagination")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VPagination : VueLibraryComponent
{
    [Parameter]
    public int ModelValue { get; set; }

    [Parameter]
    public EventCallback<int> ModelValueChanged { get; set; }

    [Parameter]
    public int Length { get; set; }

    [Parameter]
    public int? TotalVisible { get; set; }

    [Parameter]
    public bool Disabled { get; set; }
}
