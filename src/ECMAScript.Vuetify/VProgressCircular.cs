using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 圆形进度指示器组件。
/// Vuetify circular progress indicator component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VProgressCircular")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VProgressCircular : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public VuetifyProgressCircularIndeterminateValue? Indeterminate { get; set; }

    [Parameter]
    public VueStringNumberValue? ModelValue { get; set; }

    [Parameter]
    public VueStringNumberValue? Rotate { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public RenderFragment<VProgressCircularDefaultSlotContext>? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
