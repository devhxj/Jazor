using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VProgressCircular")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VProgressCircular : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Indeterminate { get; set; }

    [Parameter]
    public double? ModelValue { get; set; }
}
