using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 复选框创作代理。
/// Vuetify checkbox authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCheckbox")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
[VueSlot(nameof(Prepend), Name = "prepend")]
[VueSlot(nameof(Append), Name = "append")]
[VueSlot(nameof(Details), Name = "details")]
[VueSlot(nameof(Message), Name = "message")]
[VueSlot(nameof(LabelContent), Name = "label")]
[VueSlot(nameof(Input), Name = "input")]
public sealed class VCheckbox : VSelectionControlComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 附加到组件根元素的额外属性。
    /// Additional attributes applied to the component root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
