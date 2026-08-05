using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 下拉选择组件的编写代理。
/// Vuetify select component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSelect")]
[VueProp(nameof(SelectedValue), Name = "modelValue")]
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
