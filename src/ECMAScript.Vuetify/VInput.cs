using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 输入创作代理，用于组合验证、消息和控件插槽。
/// Vuetify input authoring proxy for composing validation, messages, and control slots.
/// </summary>
[VueLibraryComponent("vuetify/components", "VInput")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibraryEmit(nameof(PrependClick), VueEmitKind.LibrarySpecific, Name = "click:prepend")]
[VueLibraryEmit(nameof(AppendClick), VueEmitKind.LibrarySpecific, Name = "click:append")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(Details), Name = "details")]
[VueLibrarySlot(nameof(Message), Name = "message")]
public sealed class VInput : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VuetifyIconValue? PrependIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? AppendIcon { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyIconColorValue? IconColor { get; set; }

    [Parameter]
    public bool CenterAffix { get; set; }

    [Parameter]
    public bool Glow { get; set; }

    [Parameter]
    public bool HideSpinButtons { get; set; }

    [Parameter]
    public string? Hint { get; set; }

    [Parameter]
    public bool PersistentHint { get; set; }

    [Parameter]
    public VuetifyMessagesValue? Messages { get; set; }

    [Parameter]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    [Parameter]
    public bool Focused { get; set; }

    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

    [Parameter]
    public VuetifyNullableBoolean? Disabled { get; set; }

    [Parameter]
    public VuetifyNullableBoolean? Readonly { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public VuetifyMessagesValue? ErrorMessages { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxErrors { get; set; }

    [Parameter]
    public VuetifyValidationRule[]? Rules { get; set; }

    [Parameter]
    public VuetifyValidateOn? ValidateOn { get; set; }

    [Parameter]
    public VueValue? ValidationValue { get; set; }

    [Parameter]
    public VueValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VueValue?> ModelValueChanged { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> PrependClick { get; set; }

    [Parameter]
    public EventCallback<MouseEvent> AppendClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VInputSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VInputSlotContext>? Prepend { get; set; }

    [Parameter]
    public RenderFragment<VInputSlotContext>? Append { get; set; }

    [Parameter]
    public RenderFragment<VInputDetailsSlotContext>? Details { get; set; }

    [Parameter]
    public RenderFragment<VMessagesMessageSlotContext>? Message { get; set; }
}
