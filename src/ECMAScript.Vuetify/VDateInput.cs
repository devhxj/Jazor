using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify labs 日期输入创作代理。
/// Vuetify labs date-input authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VDateInput")]
public sealed class VDateInput : VInputComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 允许选择的最小日期。
    /// Minimum selectable date.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDatePickerModelValue?；值域为 Date | string | Number | VueValue[]。数值用 Min="@(32)"；变量用 Min="@value"，无需 double 后缀。字符串用 Min="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsDate、value?.AsString、value?.AsNumber、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("min")]
    public VuetifyDatePickerModelValue? Min { get; set; }

    /// <summary>
    /// 允许选择的最大日期。
    /// Maximum selectable date.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDatePickerModelValue?；值域为 Date | string | Number | VueValue[]。数值用 Max="@(32)"；变量用 Max="@value"，无需 double 后缀。字符串用 Max="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsDate、value?.AsString、value?.AsNumber、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("max")]
    public VuetifyDatePickerModelValue? Max { get; set; }

    /// <summary>
    /// 取消按钮的文本。
    /// Text for the cancel button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("cancelText")]
    public string? CancelText { get; set; }

    /// <summary>
    /// 确认按钮的文本。
    /// Text for the OK/confirm button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("okText")]
    public string? OkText { get; set; }

    /// <summary>
    /// 是否隐藏操作按钮区域。
    /// Whether to hide the actions area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideActions")]
    public bool HideActions { get; set; }

    /// <summary>
    /// 是否为移动端显示模式。
    /// Whether to display in mobile mode.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMobileValue?；值域为 bool。变量用 Mobile="@value"，并保持声明的分支类型。投影：value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("mobile")]
    public VuetifyMobileValue? Mobile { get; set; }

    /// <summary>
    /// 移动端断点阈值。
    /// Breakpoint threshold for mobile mode.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDisplayBreakpoint?；值域为 string | Number。数值用 MobileBreakpoint="@(32)"；变量用 MobileBreakpoint="@value"，无需 double 后缀。字符串用 MobileBreakpoint="text"；数字字符串保持 string。投影：value?.AsString、value?.AsNumber；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("mobileBreakpoint")]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; set; }

    /// <summary>
    /// 日期的显示格式。
    /// Display format for the date.
    /// </summary>
    /// <remarks data-authoring="types">C# union VDateInputDisplayFormatValue?；值域为 string | VDateInputDisplayFormatCallback。变量用 DisplayFormat="@value"，并保持声明的分支类型。字符串用 DisplayFormat="text"；数字字符串保持 string。投影：value?.AsString、value?.AsCallback；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("displayFormat")]
    public VDateInputDisplayFormatValue? DisplayFormat { get; set; }

    /// <summary>
    /// 选择器的弹出位置。
    /// Popup location of the picker.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 保存时触发的事件回调。
    /// Event callback fired on save.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onSave")]
    public EventCallback<string> OnSave { get; set; }

    /// <summary>
    /// 取消时触发的事件回调。
    /// Event callback fired on cancel.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onCancel")]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// 附加到组件的额外 HTML 属性。
    /// Additional HTML attributes attached to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 操作区域插槽内容。
    /// Slot content for the actions area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("actions")]
    public RenderFragment<VDateInputActionsSlotContext>? Actions { get; set; }
}
