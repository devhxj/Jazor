using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify labs icon-btn authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VIconBtn")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ActiveChanged), VueEmitKind.ModelUpdate, Name = "update:active")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Loader), Name = "loader")]
public sealed class VIconBtn : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    [Parameter]
    public bool Active { get; set; }

    [Parameter]
    public EventCallback<bool> ActiveChanged { get; set; }

    [Parameter]
    public string? ActiveColor { get; set; }

    [Parameter]
    public VuetifyIconValue? ActiveIcon { get; set; }

    [Parameter]
    public VuetifyVariant? ActiveVariant { get; set; }

    [Parameter]
    public VuetifyVariant? BaseVariant { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public bool HideOverlay { get; set; }

    [Parameter]
    public VuetifyIconValue? Icon { get; set; }

    [Parameter]
    public string? IconColor { get; set; }

    [Parameter]
    public VueStringNumberValue? IconSize { get; set; }

    [Parameter]
    public VIconBtnSizeMap? IconSizes { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public VueStringNumberValue? Opacity { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public VueStringNumberValue? Rotate { get; set; }

    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    [Parameter]
    public VIconBtnSizeMap? Sizes { get; set; }

    [Parameter]
    public VIconBtnTextValue? Text { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Loader { get; set; }
}
