using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 消息条队列组件的编写代理，用于顺序显示通知。
/// Vuetify snackbar-queue authoring proxy for sequential notifications.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSnackbarQueue")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryProp(nameof(ZIndex), Name = "zIndex")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(TextContent), Name = "text")]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
public sealed class VSnackbarQueue : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifySnackbarQueueMessages? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifySnackbarQueueMessages?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public VuetifyOverlayOffsetValue? Offset { get; set; }

    [Parameter]
    public bool Absolute { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public VuetifyOriginValue? Origin { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? Opacity { get; set; }

    [Parameter]
    public VuetifyPosition? Position { get; set; }

    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    [Parameter]
    public VueStringNumberValue? ZIndex { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public bool Eager { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VueStringNumberValue? Timeout { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public bool Vertical { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Timer { get; set; }

    [Parameter]
    public VuetifyLocationStrategy? LocationStrategy { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    [Parameter]
    public VueProps? ActivatorProps { get; set; }

    [Parameter]
    public bool OpenOnClick { get; set; }

    [Parameter]
    public bool OpenOnHover { get; set; }

    [Parameter]
    public bool OpenOnFocus { get; set; }

    [Parameter]
    public bool CloseOnContentClick { get; set; }

    [Parameter]
    public bool CloseOnBack { get; set; }

    [Parameter]
    public bool Contained { get; set; }

    [Parameter]
    public VueProps? ContentProps { get; set; }

    [Parameter]
    public VuetifyAttachTarget? Attach { get; set; }

    [Parameter]
    public bool MultiLine { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Closable { get; set; }

    [Parameter]
    public string? CloseText { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VSnackbarQueueSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VSnackbarQueueSlotContext>? TextContent { get; set; }

    [Parameter]
    public RenderFragment<VSnackbarQueueActionsSlotContext>? Actions { get; set; }
}
