using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 计数器组件创作代理。
/// Vuetify counter component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCounter")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VCounter : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Active { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    [Parameter]
    public VueStringNumberValue? Value { get; set; }

    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VCounterDefaultSlotContext>? ChildContent { get; set; }
}
