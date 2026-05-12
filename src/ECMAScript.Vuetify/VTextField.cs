using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 文本输入框组件的编写代理。
/// Vuetify text-field authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTextField")]
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
public sealed class VTextField : VInputComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 输入类型。
    /// Input type attribute.
    /// </summary>
    [Parameter]
    public string? Type { get; set; }

    /// <summary>
    /// 自动聚焦。
    /// Autofocuses the input.
    /// </summary>
    [Parameter]
    public bool Autofocus { get; set; }

    /// <summary>
    /// 反转。
    /// Reverses the input direction.
    /// </summary>
    [Parameter]
    public bool Reverse { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
