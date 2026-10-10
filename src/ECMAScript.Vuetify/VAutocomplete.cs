using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 自动补全组件。
/// Vuetify autocomplete component.
/// </summary>
[ECMAScript("vuetify/components/VAutocomplete")]
public sealed class VAutocomplete : VSelectLikeComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 组件的模型值。
    /// Model value of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public string? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化时触发的事件。
    /// Event fired when model value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 选中的值。
    /// Selected value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySelectModelValue?；值域为 string | Number | bool | Symbol | VueProps | VuetifySelectModelValue[]。数值用 SelectedValue="@(32)"；变量用 SelectedValue="@value"，无需 double 后缀。字符串用 SelectedValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifySelectModelValue? SelectedValue { get; set; }

    /// <summary>
    /// 选中值变化时触发的事件。
    /// Event fired when selected value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifySelectModelValue?；保持与模型相同的强类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifySelectModelValue?> SelectedValueChanged { get; set; }

    /// <summary>
    /// 搜索文本。
    /// Search text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("search")]
    public string? Search { get; set; }

    /// <summary>
    /// 搜索文本变化时触发的事件。
    /// Event fired when search text changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:search")]
    public EventCallback<string?> SearchChanged { get; set; }

    /// <summary>
    /// 是否自动选中第一个匹配项。
    /// Auto-selects the first matching item.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyAutoSelectFirstValue?；值域为 bool | VuetifyAutoSelectFirstMode。变量用 AutoSelectFirst="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("autoSelectFirst")]
    public VuetifyAutoSelectFirstValue? AutoSelectFirst { get; set; }

    /// <summary>
    /// 选中后是否清空搜索。
    /// Clears search on selection.
    /// </summary>
    [Parameter]
    [ECMAScriptName("clearOnSelect")]
    public bool ClearOnSelect { get; set; }

    /// <summary>
    /// 自定义过滤函数。
    /// Custom filter function.
    /// </summary>
    [Parameter]
    [ECMAScriptName("customFilter")]
    public VuetifyFilterFunction? CustomFilter { get; set; }

    /// <summary>
    /// 自定义键过滤函数。
    /// Custom key filter functions.
    /// </summary>
    [Parameter]
    [ECMAScriptName("customKeyFilter")]
    public VuetifyFilterKeyFunctions? CustomKeyFilter { get; set; }

    /// <summary>
    /// 用于过滤的键。
    /// Keys used for filtering.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyFilterKeys?；值域为 string | string[]。变量用 FilterKeys="@value"，并保持声明的分支类型。字符串用 FilterKeys="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("filterKeys")]
    public VuetifyFilterKeys? FilterKeys { get; set; }

    /// <summary>
    /// 过滤模式。
    /// Filter mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("filterMode")]
    public VuetifyFilterMode? FilterMode { get; set; }

    /// <summary>
    /// 是否禁用过滤。
    /// Disables filtering.
    /// </summary>
    [Parameter]
    [ECMAScriptName("noFilter")]
    public bool NoFilter { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}