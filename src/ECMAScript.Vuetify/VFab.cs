using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 浮动操作按钮创作代理。
/// Vuetify floating action button authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VFab")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VFab : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; } = true;

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public bool App { get; set; }

    [Parameter]
    public bool Appear { get; set; }

    [Parameter]
    public bool Extended { get; set; }

    [Parameter]
    public bool Layout { get; set; }

    [Parameter]
    public bool Offset { get; set; }

    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public VueStringNumberValue? Order { get; set; }

    [Parameter]
    public bool Absolute { get; set; }

    [Parameter]
    public bool Active { get; set; } = true;

    [Parameter]
    public string? ActiveColor { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public VuetifyTextValue? Text { get; set; }

    [Parameter]
    public string? PrependIcon { get; set; }

    [Parameter]
    public string? AppendIcon { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Loading { get; set; }

    [Parameter]
    public bool Block { get; set; }

    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public bool Exact { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? To { get; set; }

    [Parameter]
    public bool Replace { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Flat { get; set; }

    [Parameter]
    public VuetifyIconValue? Icon { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public bool Slim { get; set; }

    [Parameter]
    public bool Stacked { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VuetifyPosition? Position { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VueValue? Value { get; set; }

    [Parameter]
    public VuetifyRippleValue? Ripple { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
