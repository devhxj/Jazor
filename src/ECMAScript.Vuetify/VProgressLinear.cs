using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 线性进度条组件。
/// Vuetify linear progress bar component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VProgressLinear")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VProgressLinear : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public bool Absolute { get; set; }

    [Parameter]
    public bool Active { get; set; } = true;

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public VueStringNumberValue? BgOpacity { get; set; }

    [Parameter]
    public VueStringNumberValue? BufferValue { get; set; }

    [Parameter]
    public string? BufferColor { get; set; }

    [Parameter]
    public VueStringNumberValue? BufferOpacity { get; set; }

    [Parameter]
    public bool Clickable { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public bool Indeterminate { get; set; }

    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    [Parameter]
    public VueStringNumberValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<Number> ModelValueChanged { get; set; }

    [Parameter]
    public VueStringNumberValue? Opacity { get; set; }

    [Parameter]
    public bool Reverse { get; set; }

    [Parameter]
    public bool Stream { get; set; }

    [Parameter]
    public bool Striped { get; set; }

    [Parameter]
    public bool RoundedBar { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VProgressLinearDefaultSlotContext>? ChildContent { get; set; }
}
