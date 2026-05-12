using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// 第一波 Vuetify 面包屑导航存根，用于 RazorVue 创作。
/// First-wave Vuetify breadcrumbs stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VBreadcrumbs")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VBreadcrumbs : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyBreadcrumbItems? Items { get; set; }

    [Parameter]
    public string? Divider { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
