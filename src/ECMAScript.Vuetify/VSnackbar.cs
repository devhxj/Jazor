using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VSnackbar")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VSnackbar : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public int Timeout { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
