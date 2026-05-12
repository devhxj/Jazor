 using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 芯片组件创作代理。
/// Vuetify chip component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VChip")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(ClickClose), VueEmitKind.LibrarySpecific, Name = "click:close")]
[VueLibraryEmit(nameof(GroupSelected), VueEmitKind.LibrarySpecific, Name = "group:selected")]
[VueLibrarySlot(nameof(DefaultContent), Name = "default")]
[VueLibrarySlot(nameof(LabelContent), Name = "label")]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(Close), Name = "close")]
[VueLibrarySlot(nameof(FilterContent), Name = "filter")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VChip : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; } = true;

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public bool Replace { get; set; }

    [Parameter]
    public string? To { get; set; }

    [Parameter]
    public bool Exact { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public VuetifyGroupModelValue? Value { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    [Parameter]
    public string? ActiveClass { get; set; }

    [Parameter]
    public string? AppendAvatar { get; set; }

    [Parameter]
    public VuetifyIconValue? AppendIcon { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public bool Closable { get; set; }

    [Parameter]
    public VuetifyIconValue? CloseIcon { get; set; }

    [Parameter]
    public string? CloseLabel { get; set; }

    [Parameter]
    public bool Draggable { get; set; }

    [Parameter]
    public bool Filter { get; set; }

    [Parameter]
    public VuetifyIconValue? FilterIcon { get; set; }

    [Parameter]
    public bool Label { get; set; }

    [Parameter]
    public bool? Link { get; set; }

    [Parameter]
    public bool Pill { get; set; }

    [Parameter]
    public string? PrependAvatar { get; set; }

    [Parameter]
    public VuetifyIconValue? PrependIcon { get; set; }

    [Parameter]
    public VuetifyRippleValue? Ripple { get; set; }

    [Parameter]
    public VuetifyTextValue? Text { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> ClickClose { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupSelectedEvent> GroupSelected { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VChipDefaultSlotContext>? DefaultContent { get; set; }

    [Parameter]
    public RenderFragment? LabelContent { get; set; }

    [Parameter]
    public RenderFragment? Prepend { get; set; }

    [Parameter]
    public RenderFragment? Append { get; set; }

    [Parameter]
    public RenderFragment? Close { get; set; }

    [Parameter]
    public RenderFragment? FilterContent { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
