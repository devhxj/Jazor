using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify labs 文件上传创作代理。
/// Vuetify labs file-upload authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VFileUpload")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue", PayloadTypeName = "ECMAScript.Vue3.File[]?")]
[VueLibrarySlot(nameof(Browse), Name = "browse")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(IconContent), Name = "icon")]
[VueLibrarySlot(nameof(InputContent), Name = "input")]
[VueLibrarySlot(nameof(ItemContent), Name = "item")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(DividerContent), Name = "divider")]
public sealed class VFileUpload : ComponentBase, IVueLibraryComponent
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
    public VueStringNumberValue? Length { get; set; }

    [Parameter]
    public VueStringNumberValue? Opacity { get; set; }

    [Parameter]
    public VueStringNumberValue? Thickness { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    [Parameter]
    public string? BrowseText { get; set; }

    [Parameter]
    public string? DividerText { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public VuetifyIconValue? Icon { get; set; }

    [Parameter]
    public VuetifyFileModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<File[]?> ModelValueChanged { get; set; }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool HideBrowse { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public VuetifyScrimValue? Scrim { get; set; }

    [Parameter]
    public bool ShowSize { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VFileUploadBrowseSlotContext>? Browse { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? IconContent { get; set; }

    [Parameter]
    public RenderFragment<VFileUploadInputSlotContext>? InputContent { get; set; }

    [Parameter]
    public RenderFragment<VFileUploadItemSlotContext>? ItemContent { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    [Parameter]
    public RenderFragment? DividerContent { get; set; }
}
