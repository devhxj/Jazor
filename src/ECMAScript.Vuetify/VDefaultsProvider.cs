using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 默认值提供者创作代理，用于作用域组件默认值。
/// Vuetify defaults-provider authoring proxy for scoped component defaults.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDefaultsProvider")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VDefaultsProvider : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VueProps? Defaults { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VueStringNumberValue? Reset { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Root { get; set; }

    [Parameter]
    public bool Scoped { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
