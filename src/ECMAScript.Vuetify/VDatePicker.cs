using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 日期选择器创作代理。
/// Vuetify date-picker authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VDatePicker")]
public sealed class VDatePicker : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 选中日期的绑定值。
    /// Bound value for the selected date.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDatePickerModelValue?；值域为 Date | string | Number | VueValue[]。数值用 ModelValue="@(32)"；变量用 ModelValue="@value"，无需 double 后缀。字符串用 ModelValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsDate、value?.AsString、value?.AsNumber、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyDatePickerModelValue? ModelValue { get; set; }

    /// <summary>
    /// 选中日期变化时的回调。
    /// Callback when the selected date changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyDatePickerModelValue?；保持与模型相同的强类型。投影：value?.AsDate、value?.AsString、value?.AsNumber、value?.AsValues；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyDatePickerModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否允许多选日期。
    /// Whether to allow selecting multiple dates.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDatePickerMultipleValue?；值域为 bool | Number | VuetifyDatePickerMultipleMode | string。数值用 Multiple="@(32)"；变量用 Multiple="@value"，无需 double 后缀。字符串用 Multiple="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsMode、value?.AsCustomMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("multiple")]
    public VuetifyDatePickerMultipleValue? Multiple { get; set; }

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
    /// 当前显示的年份。
    /// Currently displayed year.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 int?；Razor 数值写 Year="@(32)" 或 Year="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("year")]
    public int? Year { get; set; }

    /// <summary>
    /// 年份变化时的回调。
    /// Callback when the displayed year changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 int；保持与模型相同的强类型。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:year")]
    public EventCallback<int> YearChanged { get; set; }

    /// <summary>
    /// 当前显示的月份。
    /// Currently displayed month.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Month="@(32)"；变量用 Month="@value"，无需 double 后缀。字符串用 Month="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("month")]
    public VueStringNumberValue? Month { get; set; }

    /// <summary>
    /// 月份变化时的回调。
    /// Callback when the displayed month changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 int；保持与模型相同的强类型。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:month")]
    public EventCallback<int> MonthChanged { get; set; }

    /// <summary>
    /// 选择器的视图模式。
    /// View mode of the picker.
    /// </summary>
    [Parameter]
    [ECMAScriptName("viewMode")]
    public VuetifyDatePickerViewMode? ViewMode { get; set; }

    /// <summary>
    /// 视图模式变化时的回调。
    /// Callback when the view mode changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:viewMode")]
    public EventCallback<VuetifyDatePickerViewMode> ViewModeChanged { get; set; }

    /// <summary>
    /// 当前激活的日期。
    /// Currently active date.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDatePickerActiveValue?；值域为 string | string[]。变量用 Active="@value"，并保持声明的分支类型。字符串用 Active="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("active")]
    public VuetifyDatePickerActiveValue? Active { get; set; }

    /// <summary>
    /// 是否禁用选择器。
    /// Whether to disable the picker.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否显示相邻月份的日期。
    /// Whether to show dates from adjacent months.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showAdjacentMonths")]
    public bool ShowAdjacentMonths { get; set; }

    /// <summary>
    /// 显示的星期列。
    /// Weekdays to display.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyCalendarWeekdays?；值域为 VuetifyCalendarWeekday[]。变量用 Weekdays="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("weekdays")]
    public VuetifyCalendarWeekdays? Weekdays { get; set; }

    /// <summary>
    /// 每月显示的周数。
    /// Number of weeks displayed per month.
    /// </summary>
    [Parameter]
    [ECMAScriptName("weeksInMonth")]
    public VuetifyDatePickerWeeksInMonth? WeeksInMonth { get; set; }

    /// <summary>
    /// 每周的第一天。
    /// First day of the week.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 FirstDayOfWeek="@(32)"；变量用 FirstDayOfWeek="@value"，无需 double 后缀。字符串用 FirstDayOfWeek="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("firstDayOfWeek")]
    public VueStringNumberValue? FirstDayOfWeek { get; set; }

    /// <summary>
    /// 允许选择的日期函数或数组。
    /// Function or array of allowed selectable dates.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDatePickerAllowedDatesValue?；值域为 VueValue[] | VuetifyDatePickerAllowedDateResolver。变量用 AllowedDates="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsDates、value?.AsResolver；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("allowedDates")]
    public VuetifyDatePickerAllowedDatesValue? AllowedDates { get; set; }

    /// <summary>
    /// 是否隐藏星期行。
    /// Whether to hide the weekday row.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideWeekdays")]
    public bool HideWeekdays { get; set; }

    /// <summary>
    /// 是否显示周数。
    /// Whether to show week numbers.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showWeek")]
    public bool ShowWeek { get; set; }

    /// <summary>
    /// 切换月份时的过渡动画。
    /// Transition animation when switching months.
    /// </summary>
    [Parameter]
    [ECMAScriptName("transition")]
    public string? Transition { get; set; }

    /// <summary>
    /// 反向切换月份时的过渡动画。
    /// Reverse transition animation when switching months.
    /// </summary>
    [Parameter]
    [ECMAScriptName("reverseTransition")]
    public string? ReverseTransition { get; set; }

    /// <summary>
    /// 导航控件的高度。
    /// Height of the navigation controls.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 ControlHeight="@(32)"；变量用 ControlHeight="@value"，无需 double 后缀。字符串用 ControlHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("controlHeight")]
    public VueStringNumberValue? ControlHeight { get; set; }

    /// <summary>
    /// 下一月导航图标。
    /// Icon for next month navigation.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 NextIcon="@value"，并保持声明的分支类型。字符串用 NextIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("nextIcon")]
    public VuetifyIconValue? NextIcon { get; set; }

    /// <summary>
    /// 上一月导航图标。
    /// Icon for previous month navigation.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 PrevIcon="@value"，并保持声明的分支类型。字符串用 PrevIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("prevIcon")]
    public VuetifyIconValue? PrevIcon { get; set; }

    /// <summary>
    /// 视图模式切换图标。
    /// Icon for switching view mode.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 ModeIcon="@value"，并保持声明的分支类型。字符串用 ModeIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modeIcon")]
    public VuetifyIconValue? ModeIcon { get; set; }

    /// <summary>
    /// 选择器的文本内容。
    /// Text content of the picker.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 头部显示文本。
    /// Header display text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("header")]
    public string? HeaderText { get; set; }

    /// <summary>
    /// 头部背景色。
    /// Header background color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("headerColor")]
    public string? HeaderColor { get; set; }

    /// <summary>
    /// 组件主题名称。
    /// Component theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 根元素 HTML 标签。
    /// Root element HTML tag.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆角样式。
    /// Border radius style.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRoundedValue?；值域为 bool | Number | string。数值用 Rounded="@(32)"；变量用 Rounded="@value"，无需 double 后缀。字符串用 Rounded="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 选择器的定位方式。
    /// Positioning of the picker.
    /// </summary>
    [Parameter]
    [ECMAScriptName("position")]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 选择器的弹出位置。
    /// Popup location of the picker.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 阴影高度级别。
    /// Elevation level.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Elevation="@(32)"；变量用 Elevation="@value"，无需 double 后缀。字符串用 Elevation="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件高度。
    /// Component height.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Height="@(32)"；变量用 Height="@value"，无需 double 后缀。字符串用 Height="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件最大高度。
    /// Maximum height of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxHeight="@(32)"；变量用 MaxHeight="@value"，无需 double 后缀。字符串用 MaxHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件最大宽度。
    /// Maximum width of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxWidth="@(32)"；变量用 MaxWidth="@value"，无需 double 后缀。字符串用 MaxWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 组件最小高度。
    /// Minimum height of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MinHeight="@(32)"；变量用 MinHeight="@value"，无需 double 后缀。字符串用 MinHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件最小宽度。
    /// Minimum width of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MinWidth="@(32)"；变量用 MinWidth="@value"，无需 double 后缀。字符串用 MinWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件宽度。
    /// Component width.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Width="@(32)"；变量用 Width="@value"，无需 double 后缀。字符串用 Width="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 边框样式。
    /// Border style.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBorderValue?；值域为 bool | Number | string。数值用 Border="@(32)"；变量用 Border="@value"，无需 double 后缀。字符串用 Border="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 组件主题色。
    /// Component theme color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 组件背景色。
    /// Component background color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bgColor")]
    public string? BgColor { get; set; }

    /// <summary>
    /// 是否显示分隔线。
    /// Whether to show dividers.
    /// </summary>
    [Parameter]
    [ECMAScriptName("divided")]
    public bool Divided { get; set; }

    /// <summary>
    /// 是否使用横向布局。
    /// Whether to use landscape layout.
    /// </summary>
    [Parameter]
    [ECMAScriptName("landscape")]
    public bool Landscape { get; set; }

    /// <summary>
    /// 选择器标题文本。
    /// Picker title text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 是否隐藏头部。
    /// Whether to hide the header.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideHeader")]
    public bool HideHeader { get; set; }

    /// <summary>
    /// 附加到组件的额外 HTML 属性。
    /// Additional HTML attributes attached to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认子内容插槽。
    /// Default child content slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 操作区域插槽内容。
    /// Slot content for the actions area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("actions")]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// 头部插槽内容。
    /// Slot content for the header area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("header")]
    public RenderFragment<VDatePickerHeaderSlotContext>? HeaderContent { get; set; }

    /// <summary>
    /// 标题插槽内容。
    /// Slot content for the title area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public RenderFragment? TitleContent { get; set; }
}
