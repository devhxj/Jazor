using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify confirm-edit authoring proxy for editable value confirmation flows.
/// </summary>
[VueLibraryComponent("vuetify/components", "VConfirmEdit")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(Save), VueEmitKind.LibrarySpecific, Name = "save")]
[VueLibraryEmit(nameof(Cancel), VueEmitKind.LibrarySpecific, Name = "cancel")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VConfirmEdit : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VueValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VueValue?> ModelValueChanged { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? CancelText { get; set; }

    [Parameter]
    public string? OkText { get; set; }

    [Parameter]
    public VuetifyConfirmEditDisabled? Disabled { get; set; }

    [Parameter]
    public bool HideActions { get; set; }

    [Parameter]
    public EventCallback<VueValue?> Save { get; set; }

    [Parameter]
    public EventCallback Cancel { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VConfirmEditSlotContext>? ChildContent { get; set; }
}
