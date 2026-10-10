using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 实验室时间选择器组件的编写代理。
/// Vuetify labs time-picker authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VTimePicker")]
public sealed class VTimePicker : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTimePickerModelValue?；值域为 string | Date。变量用 ModelValue="@value"，并保持声明的分支类型。字符串用 ModelValue="text"；数字字符串保持 string。投影：value?.AsString、value?.AsDate；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyTimePickerModelValue? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<string?> ModelValueChanged { get; set; }

    /// <summary>
    /// 允许的小时值。
    /// Allowed hours.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTimePickerAllowedUnitValue?；值域为 Number[] | VuetifyTimePickerAllowedUnitResolver。变量用 AllowedHours="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsUnits、value?.AsResolver；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("allowedHours")]
    public VuetifyTimePickerAllowedUnitValue? AllowedHours { get; set; }

    /// <summary>
    /// 允许的分钟值。
    /// Allowed minutes.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTimePickerAllowedUnitValue?；值域为 Number[] | VuetifyTimePickerAllowedUnitResolver。变量用 AllowedMinutes="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsUnits、value?.AsResolver；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("allowedMinutes")]
    public VuetifyTimePickerAllowedUnitValue? AllowedMinutes { get; set; }

    /// <summary>
    /// 允许的秒数值。
    /// Allowed seconds.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTimePickerAllowedUnitValue?；值域为 Number[] | VuetifyTimePickerAllowedUnitResolver。变量用 AllowedSeconds="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsUnits、value?.AsResolver；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("allowedSeconds")]
    public VuetifyTimePickerAllowedUnitValue? AllowedSeconds { get; set; }

    /// <summary>
    /// 标题中显示 AM/PM。
    /// Shows AM/PM in the title.
    /// </summary>
    [Parameter]
    [ECMAScriptName("ampmInTitle")]
    public bool AmpmInTitle { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables the picker.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 时间格式。
    /// Time format.
    /// </summary>
    [Parameter]
    [ECMAScriptName("format")]
    public VuetifyTimePickerFormat? Format { get; set; }

    /// <summary>
    /// 最大时间。
    /// Maximum allowed time.
    /// </summary>
    [Parameter]
    [ECMAScriptName("max")]
    public string? Max { get; set; }

    /// <summary>
    /// 最小时间。
    /// Minimum allowed time.
    /// </summary>
    [Parameter]
    [ECMAScriptName("min")]
    public string? Min { get; set; }

    /// <summary>
    /// 视图模式。
    /// View mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("viewMode")]
    public VuetifyTimePickerViewMode? ViewMode { get; set; }

    /// <summary>
    /// 视图模式变化事件。
    /// View mode changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:viewMode")]
    public EventCallback<VuetifyTimePickerViewMode> ViewModeChanged { get; set; }

    /// <summary>
    /// AM/PM变化事件。
    /// Period changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("periodChanged")]
    public EventCallback<VuetifyTimePickerPeriod> PeriodChanged { get; set; }

    /// <summary>
    /// 小时变化事件。
    /// Hour changed event.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 Number；保持与模型相同的强类型。</remarks>
    [Parameter]
    [ECMAScriptName("hourChanged")]
    public EventCallback<Number> HourChanged { get; set; }

    /// <summary>
    /// 分钟变化事件。
    /// Minute changed event.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 Number；保持与模型相同的强类型。</remarks>
    [Parameter]
    [ECMAScriptName("minuteChanged")]
    public EventCallback<Number> MinuteChanged { get; set; }

    /// <summary>
    /// 秒数变化事件。
    /// Second changed event.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 Number；保持与模型相同的强类型。</remarks>
    [Parameter]
    [ECMAScriptName("secondChanged")]
    public EventCallback<Number> SecondChanged { get; set; }

    /// <summary>
    /// 只读。
    /// Puts the picker in readonly mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 可滚动。
    /// Enables scroll interaction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scrollable")]
    public bool Scrollable { get; set; }

    /// <summary>
    /// 使用秒数。
    /// Enables seconds selection.
    /// </summary>
    [Parameter]
    [ECMAScriptName("useSeconds")]
    public bool UseSeconds { get; set; }

    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆角。
    /// Border radius.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRoundedValue?；值域为 bool | Number | string。数值用 Rounded="@(32)"；变量用 Rounded="@value"，无需 double 后缀。字符串用 Rounded="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 移除圆角。
    /// Removes border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// CSS定位。
    /// CSS position.
    /// </summary>
    [Parameter]
    [ECMAScriptName("position")]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 位置。
    /// Location of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 阴影。
    /// Elevation shadow.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Elevation="@(32)"；变量用 Elevation="@value"，无需 double 后缀。字符串用 Elevation="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 高。
    /// Height.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Height="@(32)"；变量用 Height="@value"，无需 double 后缀。字符串用 Height="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 最大高。
    /// Maximum height.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxHeight="@(32)"；变量用 MaxHeight="@value"，无需 double 后缀。字符串用 MaxHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 最大宽。
    /// Maximum width.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxWidth="@(32)"；变量用 MaxWidth="@value"，无需 double 后缀。字符串用 MaxWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 最小高。
    /// Minimum height.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MinHeight="@(32)"；变量用 MinHeight="@value"，无需 double 后缀。字符串用 MinHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 最小宽。
    /// Minimum width.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MinWidth="@(32)"；变量用 MinWidth="@value"，无需 double 后缀。字符串用 MinWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 宽。
    /// Width.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Width="@(32)"；变量用 Width="@value"，无需 double 后缀。字符串用 Width="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 边框。
    /// Border.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBorderValue?；值域为 bool | Number | string。数值用 Border="@(32)"；变量用 Border="@value"，无需 double 后缀。字符串用 Border="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 主题颜色。
    /// Theme color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 背景颜色。
    /// Background color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bgColor")]
    public string? BgColor { get; set; }

    /// <summary>
    /// 分隔线。
    /// Adds a divider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("divided")]
    public bool Divided { get; set; }

    /// <summary>
    /// 标题。
    /// Title text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 隐藏头部。
    /// Hides the header.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideHeader")]
    public bool HideHeader { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 操作插槽。
    /// Actions slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("actions")]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// 标题内容插槽。
    /// Title content slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public RenderFragment? TitleContent { get; set; }
}
