using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify labs date-input authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VDateInput")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibraryEmit(nameof(Save), VueEmitKind.LibrarySpecific, Name = "save")]
[VueLibraryEmit(nameof(Cancel), VueEmitKind.LibrarySpecific, Name = "cancel")]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(PrependInner), Name = "prepend-inner")]
[VueLibrarySlot(nameof(AppendInner), Name = "append-inner")]
[VueLibrarySlot(nameof(Clear), Name = "clear")]
[VueLibrarySlot(nameof(LabelContent), Name = "label")]
[VueLibrarySlot(nameof(Details), Name = "details")]
[VueLibrarySlot(nameof(CounterContent), Name = "counter")]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
public sealed class VDateInput : VInputComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyDatePickerModelValue? Min { get; set; }

    [Parameter]
    public VuetifyDatePickerModelValue? Max { get; set; }

    [Parameter]
    public string? CancelText { get; set; }

    [Parameter]
    public string? OkText { get; set; }

    [Parameter]
    public bool HideActions { get; set; }

    [Parameter]
    public VuetifyMobileValue? Mobile { get; set; }

    [Parameter]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; set; }

    [Parameter]
    public VDateInputDisplayFormatValue? DisplayFormat { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public EventCallback<string> Save { get; set; }

    [Parameter]
    public EventCallback Cancel { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VDateInputActionsSlotContext>? Actions { get; set; }
}
