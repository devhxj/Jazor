using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify color-picker authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VColorPicker")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(ModeChanged), VueEmitKind.ModelUpdate, Name = "update:mode")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Header), Name = "header")]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
public sealed class VColorPicker : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyColorValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyColorValue?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyColorPickerMode? Mode { get; set; }

    [Parameter]
    public EventCallback<VuetifyColorPickerMode> ModeChanged { get; set; }

    [Parameter]
    public VuetifyColorPickerModes? Modes { get; set; }

    [Parameter]
    public VueStringNumberValue? CanvasHeight { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VueStringNumberValue? DotSize { get; set; }

    [Parameter]
    public bool HideCanvas { get; set; }

    [Parameter]
    public bool HideSliders { get; set; }

    [Parameter]
    public bool HideInputs { get; set; }

    [Parameter]
    public bool ShowSwatches { get; set; }

    [Parameter]
    public VuetifyColorPickerSwatches? Swatches { get; set; }

    [Parameter]
    public VueStringNumberValue? SwatchesMaxHeight { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public VuetifyPosition? Position { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public bool Divided { get; set; }

    [Parameter]
    public bool Landscape { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public bool HideHeader { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }
}
