using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VSelect")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryProp(nameof(SelectedValue), VuePropKind.Model, Name = "modelValue", AcceptsBinding = true)]
[VueLibraryEmit(nameof(SelectedValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibraryEmit(nameof(MenuChanged), VueEmitKind.ModelUpdate, Name = "update:menu")]
[VueLibrarySlot(nameof(Item), Name = "item")]
[VueLibrarySlot(nameof(Chip), Name = "chip")]
[VueLibrarySlot(nameof(Selection), Name = "selection")]
[VueLibrarySlot(nameof(PrependItem), Name = "prepend-item")]
[VueLibrarySlot(nameof(AppendItem), Name = "append-item")]
[VueLibrarySlot(nameof(NoData), Name = "no-data")]
public sealed class VSelect : VSelectLikeComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifySelectModelValue? SelectedValue { get; set; }

    [Parameter]
    public EventCallback<VuetifySelectModelValue?> SelectedValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
