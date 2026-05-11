using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify field authoring proxy for composing custom input chrome.
/// </summary>
[VueLibraryComponent("vuetify/components", "VField")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibraryEmit(nameof(ClearClick), VueEmitKind.LibrarySpecific, Name = "click:clear")]
[VueLibraryEmit(nameof(AppendInnerClick), VueEmitKind.LibrarySpecific, Name = "click:appendInner")]
[VueLibraryEmit(nameof(PrependInnerClick), VueEmitKind.LibrarySpecific, Name = "click:prependInner")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(PrependInner), Name = "prepend-inner")]
[VueLibrarySlot(nameof(AppendInner), Name = "append-inner")]
[VueLibrarySlot(nameof(Clear), Name = "clear")]
[VueLibrarySlot(nameof(LabelContent), Name = "label")]
[VueLibrarySlot(nameof(Loader), Name = "loader")]
public sealed class VField : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Loading { get; set; }

    [Parameter]
    public VuetifyIconValue? AppendInnerIcon { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public VuetifyIconValue? ClearIcon { get; set; }

    [Parameter]
    public bool Active { get; set; }

    [Parameter]
    public bool? CenterAffix { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public bool Dirty { get; set; }

    [Parameter]
    public bool? Disabled { get; set; }

    [Parameter]
    public bool Glow { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public bool Flat { get; set; }

    [Parameter]
    public VuetifyIconColorValue? IconColor { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool PersistentClear { get; set; }

    [Parameter]
    public VuetifyIconValue? PrependInnerIcon { get; set; }

    [Parameter]
    public bool Reverse { get; set; }

    [Parameter]
    public bool SingleLine { get; set; }

    [Parameter]
    public VuetifyFieldVariant? Variant { get; set; }

    [Parameter]
    public bool Focused { get; set; }

    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

    [Parameter]
    public VueValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VueValue?> ModelValueChanged { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> ClearClick { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> AppendInnerClick { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> PrependInnerClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VFieldSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VFieldSlotContext>? PrependInner { get; set; }

    [Parameter]
    public RenderFragment<VFieldSlotContext>? AppendInner { get; set; }

    [Parameter]
    public RenderFragment<VFieldSlotContext>? Clear { get; set; }

    [Parameter]
    public RenderFragment<VFieldLabelSlotContext>? LabelContent { get; set; }

    [Parameter]
    public RenderFragment<VuetifyLoaderSlotContext>? Loader { get; set; }
}
