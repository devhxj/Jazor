using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 分隔线组件。
/// Vuetify divider component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDivider")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VDivider : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Inset { get; set; }

    [Parameter]
    public int? Thickness { get; set; }

    [Parameter]
    public bool Vertical { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
