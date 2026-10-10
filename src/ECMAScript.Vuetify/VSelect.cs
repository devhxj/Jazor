using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 下拉选择组件的编写代理。
/// Vuetify select component authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VSelect")]
public sealed class VSelect : VSelectLikeComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 当前选中的值。
    /// The currently selected value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public string? ModelValue { get; set; }

    /// <summary>
    /// 选中值变更时触发的回调。
    /// Callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 绑定到 v-model 的选中项值。
    /// The selected item value bound to v-model.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySelectModelValue?；值域为 string | Number | bool | Symbol | VueProps | VuetifySelectModelValue[]。数值用 SelectedValue="@(32)"；变量用 SelectedValue="@value"，无需 double 后缀。字符串用 SelectedValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifySelectModelValue? SelectedValue { get; set; }

    /// <summary>
    /// 选中项值变更时触发的回调。
    /// Callback invoked when the selected item value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifySelectModelValue?；保持与模型相同的强类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifySelectModelValue?> SelectedValueChanged { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
