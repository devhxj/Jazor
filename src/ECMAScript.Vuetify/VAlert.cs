using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VAlert")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(ClickClose), VueEmitKind.LibrarySpecific, Name = "click:close")]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(TextContent), Name = "text")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(Close), Name = "close")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VAlert : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; } = true;

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyAlertType? Type { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public string? Color { get; set; }

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
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public VuetifyAlertBorderValue? Border { get; set; }

    [Parameter]
    public string? BorderColor { get; set; }

    [Parameter]
    public bool Closable { get; set; }

    [Parameter]
    public VuetifyIconValue? CloseIcon { get; set; }

    [Parameter]
    public string? CloseLabel { get; set; }

    [Parameter]
    public VuetifyAlertIconValue? Icon { get; set; }

    [Parameter]
    public bool Prominent { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> ClickClose { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? Prepend { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    [Parameter]
    public RenderFragment? TextContent { get; set; }

    [Parameter]
    public RenderFragment? Append { get; set; }

    [Parameter]
    public RenderFragment<VAlertCloseSlotContext>? Close { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
