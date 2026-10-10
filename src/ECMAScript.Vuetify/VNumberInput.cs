using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 数字输入组件。
/// Vuetify number input component.
/// </summary>
[ECMAScript("vuetify/components/VNumberInput")]
public sealed class VNumberInput : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 数字输入控件的标签文本。
    /// Label text of the number input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 控制按钮是否内嵌显示。
    /// Whether to display control buttons in an inset style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("inset")]
    public bool Inset { get; set; }

    /// <summary>
    /// 是否反转控制按钮的排列顺序。
    /// Whether to reverse the order of control buttons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("reverse")]
    public bool Reverse { get; set; }

    /// <summary>
    /// 是否隐藏输入框，仅显示控制按钮。
    /// Whether to hide the input field, showing only control buttons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideInput")]
    public bool HideInput { get; set; }

    /// <summary>
    /// 控制按钮的视觉变体样式。
    /// Visual variant style of the control buttons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("controlVariant")]
    public VuetifyNumberInputControlVariant? ControlVariant { get; set; }

    /// <summary>
    /// 允许输入的最小值。
    /// Minimum allowed value.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 Min="@(32)" 或 Min="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("min")]
    public Number? Min { get; set; }

    /// <summary>
    /// 允许输入的最大值。
    /// Maximum allowed value.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 Max="@(32)" 或 Max="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("max")]
    public Number? Max { get; set; }

    /// <summary>
    /// 每次增减的步长值。
    /// Step increment/decrement value.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 Step="@(32)" 或 Step="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("step")]
    public Number? Step { get; set; }

    /// <summary>
    /// 数值的小数精度位数。
    /// Decimal precision of the numeric value.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 Precision="@(32)" 或 Precision="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("precision")]
    public Number? Precision { get; set; }

    /// <summary>
    /// 是否显示清除按钮。
    /// Whether to show a clear button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("clearable")]
    public bool Clearable { get; set; }

    /// <summary>
    /// 是否禁用输入控件。
    /// Whether to disable the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否将输入控件设为只读。
    /// Whether to make the input control read-only.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 组件的密度样式，调整垂直间距。
    /// Component density style that adjusts vertical spacing.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 输入控件的视觉变体样式。
    /// Visual variant style of the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyFieldVariant? Variant { get; set; }

    /// <summary>
    /// 输入框的占位文本。
    /// Placeholder text of the input field.
    /// </summary>
    [Parameter]
    [ECMAScriptName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    /// 输入控件的提示文本。
    /// Hint text for the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hint")]
    public string? Hint { get; set; }

    /// <summary>
    /// 是否在未聚焦时持续显示提示文本。
    /// Whether to persistently show the hint text when not focused.
    /// </summary>
    [Parameter]
    [ECMAScriptName("persistentHint")]
    public bool PersistentHint { get; set; }

    /// <summary>
    /// 是否隐藏提示详情区域。
    /// Whether to hide the details area.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyHideDetailsValue?；值域为 bool | VuetifyHideDetailsMode。变量用 HideDetails="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("hideDetails")]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    /// <summary>
    /// 显示在输入控件下方的消息列表。
    /// Messages displayed below the input control.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMessagesValue?；值域为 string | string[]。变量用 Messages="@value"，并保持声明的分支类型。字符串用 Messages="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("messages")]
    public VuetifyMessagesValue? Messages { get; set; }

    /// <summary>
    /// 数字输入控件的双向绑定值。
    /// Two-way bound value of the number input control.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 ModelValue="@(32)" 或 ModelValue="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public Number? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变更时触发的回调。
    /// Callback invoked when the bound value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 Number?；保持与模型相同的强类型。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<Number?> ModelValueChanged { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
