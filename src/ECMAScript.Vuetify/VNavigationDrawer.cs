using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VNavigationDrawer")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VNavigationDrawer : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool? ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool?> ModelValueChanged { get; set; }

    [Parameter]
    public bool? Rail { get; set; }

    [Parameter]
    public EventCallback<bool?> RailChanged { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Permanent { get; set; }

    [Parameter]
    public bool Temporary { get; set; }

    [Parameter]
    public bool Persistent { get; set; }

    [Parameter]
    public bool ExpandOnHover { get; set; }

    [Parameter]
    public bool Floating { get; set; }

    [Parameter]
    public bool Sticky { get; set; }

    [Parameter]
    public bool Touchless { get; set; }

    [Parameter]
    public bool DisableResizeWatcher { get; set; }

    [Parameter]
    public bool DisableRouteWatcher { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VueStringNumberValue? RailWidth { get; set; }

    [Parameter]
    public VuetifyScrimValue? Scrim { get; set; }

    [Parameter]
    public string? Image { get; set; }

    [Parameter]
    public VuetifyNavigationDrawerLocation? Location { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
