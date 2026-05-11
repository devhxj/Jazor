using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VTextarea")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(PrependInner), Name = "prepend-inner")]
[VueLibrarySlot(nameof(AppendInner), Name = "append-inner")]
[VueLibrarySlot(nameof(Clear), Name = "clear")]
[VueLibrarySlot(nameof(LabelContent), Name = "label")]
[VueLibrarySlot(nameof(Details), Name = "details")]
[VueLibrarySlot(nameof(CounterContent), Name = "counter")]
public sealed class VTextarea : VInputComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool AutoGrow { get; set; }

    [Parameter]
    public VueStringNumberValue? Rows { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxRows { get; set; }

    [Parameter]
    public bool NoResize { get; set; }

    [Parameter]
    public bool Autofocus { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
