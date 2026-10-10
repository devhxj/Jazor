using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 组合框组件创作代理。
/// Vuetify combobox component authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VCombobox")]
public sealed class VCombobox : VSelectLikeComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 是否自动选中第一个匹配项。
    /// Whether to auto-select the first matching item.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyAutoSelectFirstValue?；值域为 bool | VuetifyAutoSelectFirstMode。变量用 AutoSelectFirst="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("autoSelectFirst")]
    public VuetifyAutoSelectFirstValue? AutoSelectFirst { get; set; }

    /// <summary>
    /// 选中后是否清除搜索文本。
    /// Whether to clear the search text after selection.
    /// </summary>
    [Parameter]
    [ECMAScriptName("clearOnSelect")]
    public bool ClearOnSelect { get; set; } = true;

    /// <summary>
    /// 多选分隔符数组。
    /// The array of delimiters for multiple selection.
    /// </summary>
    [Parameter]
    [ECMAScriptName("delimiters")]
    public string[]? Delimiters { get; set; }

    /// <summary>
    /// 当前搜索文本。
    /// The current search text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("search")]
    public string? Search { get; set; }

    /// <summary>
    /// 搜索文本变更回调。
    /// Callback invoked when the search text changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:search")]
    public EventCallback<string?> SearchChanged { get; set; }

    /// <summary>
    /// 自定义筛选函数。
    /// The custom filter function.
    /// </summary>
    [Parameter]
    [ECMAScriptName("customFilter")]
    public VuetifyFilterFunction? CustomFilter { get; set; }

    /// <summary>
    /// 自定义按键筛选函数集合。
    /// The custom key-filter functions.
    /// </summary>
    [Parameter]
    [ECMAScriptName("customKeyFilter")]
    public VuetifyFilterKeyFunctions? CustomKeyFilter { get; set; }

    /// <summary>
    /// 用于筛选的字段键。
    /// The keys used for filtering items.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyFilterKeys?；值域为 string | string[]。变量用 FilterKeys="@value"，并保持声明的分支类型。字符串用 FilterKeys="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("filterKeys")]
    public VuetifyFilterKeys? FilterKeys { get; set; }

    /// <summary>
    /// 筛选匹配模式。
    /// The filter matching mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("filterMode")]
    public VuetifyFilterMode? FilterMode { get; set; }

    /// <summary>
    /// 是否禁用筛选。
    /// Whether to disable filtering.
    /// </summary>
    [Parameter]
    [ECMAScriptName("noFilter")]
    public bool NoFilter { get; set; }

    /// <summary>
    /// 组合框文本绑定值。
    /// The bound text value of the combobox.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public string? ModelValue { get; set; }

    /// <summary>
    /// 文本值变更回调。
    /// Callback invoked when the text value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 选中项绑定值。
    /// The bound selected value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySelectModelValue?；值域为 string | Number | bool | Symbol | VueProps | VuetifySelectModelValue[]。数值用 SelectedValue="@(32)"；变量用 SelectedValue="@value"，无需 double 后缀。字符串用 SelectedValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifySelectModelValue? SelectedValue { get; set; }

    /// <summary>
    /// 选中项变更回调。
    /// Callback invoked when the selected value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifySelectModelValue?；保持与模型相同的强类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifySelectModelValue?> SelectedValueChanged { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
