using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify breadcrumbs stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VBreadcrumbs")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VBreadcrumbs : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyBreadcrumbItems? Items { get; set; }

    [Parameter]
    public string? Divider { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
