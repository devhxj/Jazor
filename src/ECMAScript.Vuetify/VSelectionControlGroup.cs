using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 选择控件分组组件的编写代理。
/// Vuetify selection-control group authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VSelectionControlGroup")]
public sealed class VSelectionControlGroup : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 分组的唯一标识符。
    /// The unique identifier for the group.
    /// </summary>
    [Parameter]
    [ECMAScriptName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 表单元素的 name 属性。
    /// The name attribute for the form element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 输入控件的类型。
    /// The type of the input controls.
    /// </summary>
    [Parameter]
    [ECMAScriptName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// The theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 默认提供者的目标属性名。
    /// The target property name for the defaults provider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("defaultsTarget")]
    public string? DefaultsTarget { get; set; }

    /// <summary>
    /// 控件组的颜色。
    /// The color of the control group.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// The density/compactness of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否禁用整个控件组。
    /// Whether to disable the entire control group.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyNullableBoolean?；值域为 bool。变量用 Disabled="@value"，并保持声明的分支类型。投影：value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("disabled")]
    public VuetifyNullableBoolean? Disabled { get; set; }

    /// <summary>
    /// 是否为只读状态。
    /// Whether the controls are read-only.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyNullableBoolean?；值域为 bool。变量用 Readonly="@value"，并保持声明的分支类型。投影：value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("readonly")]
    public VuetifyNullableBoolean? Readonly { get; set; }

    /// <summary>
    /// 是否处于错误状态。
    /// Whether the group is in an error state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("error")]
    public bool Error { get; set; }

    /// <summary>
    /// 是否将控件横向排列。
    /// Whether to display controls inline horizontally.
    /// </summary>
    [Parameter]
    [ECMAScriptName("inline")]
    public bool Inline { get; set; }

    /// <summary>
    /// 是否支持多选。
    /// Whether to support multiple selections.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyNullableBoolean?；值域为 bool。变量用 Multiple="@value"，并保持声明的分支类型。投影：value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("multiple")]
    public VuetifyNullableBoolean? Multiple { get; set; }

    /// <summary>
    /// 未选中状态下显示的图标。
    /// The icon displayed when unchecked.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 FalseIcon="@value"，并保持声明的分支类型。字符串用 FalseIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("falseIcon")]
    public VuetifyIconValue? FalseIcon { get; set; }

    /// <summary>
    /// 选中状态下显示的图标。
    /// The icon displayed when checked.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 TrueIcon="@value"，并保持声明的分支类型。字符串用 TrueIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("trueIcon")]
    public VuetifyIconValue? TrueIcon { get; set; }

    /// <summary>
    /// 是否启用涟漪效果。
    /// Whether to enable the ripple effect.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRippleValue?；值域为 bool | VueProps。变量用 Ripple="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsOptions；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("ripple")]
    public VuetifyRippleValue? Ripple { get; set; }

    /// <summary>
    /// 用于比较值的自定义比较器。
    /// The custom comparator used for value comparison.
    /// </summary>
    [Parameter]
    [ECMAScriptName("valueComparator")]
    public VuetifyValueComparator? ValueComparator { get; set; }

    /// <summary>
    /// 控件组的当前绑定值。
    /// The current bound value of the control group.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGroupModelValue?；值域为 string | Number | bool | Symbol | VueProps | VuetifyGroupModelValue[]。数值用 ModelValue="@(32)"；变量用 ModelValue="@value"，无需 double 后缀。字符串用 ModelValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变更时触发的回调。
    /// Callback invoked when the bound value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyGroupModelValue?；保持与模型相同的强类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容，用于放置选择控件。
    /// The default slot for placing selection controls.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
