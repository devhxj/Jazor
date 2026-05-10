using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First-wave Vuetify data table stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDataTable")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VDataTable : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyDataTableHeaders? Headers { get; set; }

    [Parameter]
    public VuetifyDataTableItems? Items { get; set; }

    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public string? ItemKey { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
