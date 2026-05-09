using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// First scoped-slot Vuetify example for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDialog")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VDialog : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public bool Persistent { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VuetifyScrollStrategy? ScrollStrategy { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public string? Transition { get; set; }

    [Parameter]
    public RenderFragment<VDialogActivatorContext>? Activator { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
