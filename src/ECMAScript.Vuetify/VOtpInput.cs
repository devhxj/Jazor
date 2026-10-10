using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 一次性密码输入组件。
/// Vuetify OTP input component.
/// </summary>
[ECMAScript("vuetify/components/VOtpInput")]
public sealed class VOtpInput : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// OTP 输入框的数量。
    /// Number of OTP input fields.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Length="@(32)"；变量用 Length="@value"，无需 double 后缀。字符串用 Length="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("length")]
    public VueStringNumberValue? Length { get; set; }

    /// <summary>
    /// 是否自动聚焦第一个输入框。
    /// Whether to automatically focus the first input field.
    /// </summary>
    [Parameter]
    [ECMAScriptName("autofocus")]
    public bool Autofocus { get; set; }

    /// <summary>
    /// 输入框之间的分隔符。
    /// Divider between input fields.
    /// </summary>
    [Parameter]
    [ECMAScriptName("divider")]
    public string? Divider { get; set; }

    /// <summary>
    /// 是否同时聚焦所有输入框。
    /// Whether to focus all input fields simultaneously.
    /// </summary>
    [Parameter]
    [ECMAScriptName("focusAll")]
    public bool FocusAll { get; set; }

    /// <summary>
    /// 是否显示加载状态。
    /// Whether to show a loading state.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBooleanStringValue?；值域为 bool | string。变量用 Loading="@value"，并保持声明的分支类型。字符串用 Loading="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("loading")]
    public VuetifyBooleanStringValue? Loading { get; set; }

    /// <summary>
    /// 输入控件的视觉变体样式。
    /// Visual variant style of the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyFieldVariant? Variant { get; set; }

    /// <summary>
    /// 组件的密度样式，调整垂直间距。
    /// Component density style that adjusts vertical spacing.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否禁用输入控件。
    /// Whether to disable the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否将输入控件置于错误状态。
    /// Whether to put the input control in an error state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("error")]
    public bool Error { get; set; }

    /// <summary>
    /// 输入框的 HTML 输入类型。
    /// HTML input type of the input fields.
    /// </summary>
    [Parameter]
    [ECMAScriptName("type")]
    public VuetifyInputType? Type { get; set; }

    /// <summary>
    /// OTP 输入控件的双向绑定值。
    /// Two-way bound value of the OTP input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public string? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变更时触发的回调。
    /// Callback invoked when the bound value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
