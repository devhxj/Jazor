using Jazor.RazorVue;
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
    public IEnumerable<object>? Headers { get; set; }

    [Parameter]
    public IEnumerable<object>? Items { get; set; }

    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public string? ItemKey { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
