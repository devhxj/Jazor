using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VAutocomplete")]
/// <summary>
/// Vuetify 自动补全组件。
/// Vuetify autocomplete component.
/// </summary>
public sealed class VAutocomplete : VSelectLikeComponentBase
{
    /// <summary>
    /// 组件的模型值。
    /// Model value of the component.
    /// </summary>
    [Parameter]
    public string? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化时触发的事件。
    /// Event fired when model value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 选中的值。
    /// Selected value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifySelectModelValue? SelectedValue { get; set; }

    /// <summary>
    /// 选中值变化时触发的事件。
    /// Event fired when selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifySelectModelValue?> SelectedValueChanged { get; set; }

    /// <summary>
    /// 搜索文本。
    /// Search text.
    /// </summary>
    [Parameter]
    public string? Search { get; set; }

    /// <summary>
    /// 搜索文本变化时触发的事件。
    /// Event fired when search text changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> SearchChanged { get; set; }

    /// <summary>
    /// 是否自动选中第一个匹配项。
    /// Auto-selects the first matching item.
    /// </summary>
    [Parameter]
    public VuetifyAutoSelectFirstValue? AutoSelectFirst { get; set; }

    /// <summary>
    /// 选中后是否清空搜索。
    /// Clears search on selection.
    /// </summary>
    [Parameter]
    public bool ClearOnSelect { get; set; }

    /// <summary>
    /// 自定义过滤函数。
    /// Custom filter function.
    /// </summary>
    [Parameter]
    public VuetifyFilterFunction? CustomFilter { get; set; }

    /// <summary>
    /// 自定义键过滤函数。
    /// Custom key filter functions.
    /// </summary>
    [Parameter]
    public VuetifyFilterKeyFunctions? CustomKeyFilter { get; set; }

    /// <summary>
    /// 用于过滤的键。
    /// Keys used for filtering.
    /// </summary>
    [Parameter]
    public VuetifyFilterKeys? FilterKeys { get; set; }

    /// <summary>
    /// 过滤模式。
    /// Filter mode.
    /// </summary>
    [Parameter]
    public VuetifyFilterMode? FilterMode { get; set; }

    /// <summary>
    /// 是否禁用过滤。
    /// Disables filtering.
    /// </summary>
    [Parameter]
    public bool NoFilter { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
