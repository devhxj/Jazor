using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VAlert")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VAlert : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Type { get; set; }

    [Parameter]
    public string? Variant { get; set; }

    [Parameter]
    public bool Closable { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
