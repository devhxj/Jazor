using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VSwitch")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(Details), Name = "details")]
[VueLibrarySlot(nameof(Message), Name = "message")]
[VueLibrarySlot(nameof(LabelContent), Name = "label")]
[VueLibrarySlot(nameof(Input), Name = "input")]
[VueLibrarySlot(nameof(Loader), Name = "loader")]
[VueLibrarySlot(nameof(Thumb), Name = "thumb")]
[VueLibrarySlot(nameof(TrackTrue), Name = "track-true")]
[VueLibrarySlot(nameof(TrackFalse), Name = "track-false")]
public sealed class VSwitch : VSelectionControlComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Inset { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Loading { get; set; }

    [Parameter]
    public bool Flat { get; set; }

    [Parameter]
    public RenderFragment<VuetifyLoaderSlotContext>? Loader { get; set; }

    [Parameter]
    public RenderFragment<VSwitchSlotContext>? Thumb { get; set; }

    [Parameter]
    public RenderFragment<VSwitchSlotContext>? TrackTrue { get; set; }

    [Parameter]
    public RenderFragment<VSwitchSlotContext>? TrackFalse { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
