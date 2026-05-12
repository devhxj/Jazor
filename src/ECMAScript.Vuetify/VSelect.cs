using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 下拉选择组件的编写代理。
/// Vuetify select component authoring proxy.
/// </summary>
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
    /// <summary>
    /// 当前选中的值。
    /// The currently selected value.
    /// </summary>
    [Parameter]
    public string? ModelValue { get; set; }

    /// <summary>
    /// 选中值变更时触发的回调。
    /// Callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 绑定到 v-model 的选中项值。
    /// The selected item value bound to v-model.
    /// </summary>
    [Parameter]
    public VuetifySelectModelValue? SelectedValue { get; set; }

    /// <summary>
    /// 选中项值变更时触发的回调。
    /// Callback invoked when the selected item value changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifySelectModelValue?> SelectedValueChanged { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
