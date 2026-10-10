using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify Labs 日历创作代理。
/// Vuetify labs calendar authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VCalendar")]
public sealed class VCalendar : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 日历的当前日期值。
    /// Current date value of the calendar.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyCalendarDateValues?；值域为 VuetifyCalendarDateValue[]。变量用 ModelValue="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyCalendarDateValues? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化时触发的事件。
    /// Event fired when model value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyCalendarDateValues?；保持与模型相同的强类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyCalendarDateValues?> ModelValueChanged { get; set; }

    /// <summary>
    /// 下一页图标。
    /// Next page icon.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 NextIcon="@value"，并保持声明的分支类型。字符串用 NextIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("nextIcon")]
    public VuetifyIconValue? NextIcon { get; set; }

    /// <summary>
    /// 上一页图标。
    /// Previous page icon.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 PrevIcon="@value"，并保持声明的分支类型。字符串用 PrevIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("prevIcon")]
    public VuetifyIconValue? PrevIcon { get; set; }

    /// <summary>
    /// 标题文本。
    /// Title text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 文本内容。
    /// Text content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 日历的视图模式。
    /// Calendar view mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("viewMode")]
    public VuetifyCalendarViewMode? ViewMode { get; set; }

    /// <summary>
    /// 日期信息对象。
    /// Day information object.
    /// </summary>
    [Parameter]
    [ECMAScriptName("day")]
    public VuetifyCalendarDay? Day { get; set; }

    /// <summary>
    /// 日期索引。
    /// Day index.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 DayIndex="@(32)" 或 DayIndex="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("dayIndex")]
    public Number? DayIndex { get; set; }

    /// <summary>
    /// 事件列表。
    /// Events list.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyCalendarEvents?；值域为 VuetifyCalendarEventItem[]。变量用 Events="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("events")]
    public VuetifyCalendarEvents? Events { get; set; }

    /// <summary>
    /// 时间间隔的分割数。
    /// Number of divisions per interval.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 IntervalDivisions="@(32)" 或 IntervalDivisions="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("intervalDivisions")]
    public Number? IntervalDivisions { get; set; }

    /// <summary>
    /// 时间间隔的持续时间（分钟）。
    /// Interval duration in minutes.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 IntervalDuration="@(32)" 或 IntervalDuration="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("intervalDuration")]
    public Number? IntervalDuration { get; set; }

    /// <summary>
    /// 时间间隔的像素高度。
    /// Interval height in pixels.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 IntervalHeight="@(32)" 或 IntervalHeight="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("intervalHeight")]
    public Number? IntervalHeight { get; set; }

    /// <summary>
    /// 时间间隔的格式化方式。
    /// Interval format.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyCalendarIntervalFormatValue?；值域为 string | VuetifyCalendarIntervalFormatter。变量用 IntervalFormat="@value"，并保持声明的分支类型。字符串用 IntervalFormat="text"；数字字符串保持 string。投影：value?.AsFormat、value?.AsFormatter；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("intervalFormat")]
    public VuetifyCalendarIntervalFormatValue? IntervalFormat { get; set; }

    /// <summary>
    /// 日开始的时间间隔索引。
    /// Interval start index for the day.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 IntervalStart="@(32)" 或 IntervalStart="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("intervalStart")]
    public Number? IntervalStart { get; set; }

    /// <summary>
    /// 是否隐藏日期头部。
    /// Hides day header.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideDayHeader")]
    public bool HideDayHeader { get; set; }

    /// <summary>
    /// 每天的时间间隔数量。
    /// Number of intervals per day.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 Number?；Razor 数值写 Intervals="@(32)" 或 Intervals="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("intervals")]
    public Number? Intervals { get; set; }

    /// <summary>
    /// 允许选择的日期。
    /// Allowed dates for selection.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyCalendarAllowedDatesValue?；值域为 VuetifyCalendarDateValue[] | VuetifyCalendarAllowedDateResolver。变量用 AllowedDates="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsDates、value?.AsResolver；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("allowedDates")]
    public VuetifyCalendarAllowedDatesValue? AllowedDates { get; set; }

    /// <summary>
    /// 是否禁用组件。
    /// Disables the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 当前显示的日期值。
    /// Currently displayed date value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyCalendarDateValue?；值域为 Date | string | Number。数值用 DisplayValue="@(32)"；变量用 DisplayValue="@value"，无需 double 后缀。字符串用 DisplayValue="text"；数字字符串保持 string。投影：value?.AsDate、value?.AsString、value?.AsNumber；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("displayValue")]
    public VuetifyCalendarDateValue? DisplayValue { get; set; }

    /// <summary>
    /// 显示的月份。
    /// Month to display.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Month="@(32)"；变量用 Month="@value"，无需 double 后缀。字符串用 Month="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("month")]
    public VueStringNumberValue? Month { get; set; }

    /// <summary>
    /// 最大日期值。
    /// Maximum date value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyCalendarDateValue?；值域为 Date | string | Number。数值用 Max="@(32)"；变量用 Max="@value"，无需 double 后缀。字符串用 Max="text"；数字字符串保持 string。投影：value?.AsDate、value?.AsString、value?.AsNumber；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("max")]
    public VuetifyCalendarDateValue? Max { get; set; }

    /// <summary>
    /// 最小日期值。
    /// Minimum date value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyCalendarDateValue?；值域为 Date | string | Number。数值用 Min="@(32)"；变量用 Min="@value"，无需 double 后缀。字符串用 Min="text"；数字字符串保持 string。投影：value?.AsDate、value?.AsString、value?.AsNumber；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("min")]
    public VuetifyCalendarDateValue? Min { get; set; }

    /// <summary>
    /// 是否显示相邻月份的日期。
    /// Shows adjacent month dates.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showAdjacentMonths")]
    public bool ShowAdjacentMonths { get; set; }

    /// <summary>
    /// 显示的年份。
    /// Year to display.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Year="@(32)"；变量用 Year="@value"，无需 double 后缀。字符串用 Year="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("year")]
    public VueStringNumberValue? Year { get; set; }

    /// <summary>
    /// 一周中显示的星期。
    /// Weekdays to display.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyCalendarWeekdays?；值域为 VuetifyCalendarWeekday[]。变量用 Weekdays="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("weekdays")]
    public VuetifyCalendarWeekdays? Weekdays { get; set; }

    /// <summary>
    /// 月份中的周数计算模式。
    /// Weeks in month calculation mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("weeksInMonth")]
    public VuetifyCalendarWeeksInMonth? WeeksInMonth { get; set; }

    /// <summary>
    /// 一周的第一天。
    /// First day of the week.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 FirstDayOfWeek="@(32)"；变量用 FirstDayOfWeek="@value"，无需 double 后缀。字符串用 FirstDayOfWeek="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("firstDayOfWeek")]
    public VueStringNumberValue? FirstDayOfWeek { get; set; }

    /// <summary>
    /// 是否隐藏头部。
    /// Hides the header.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideHeader")]
    public bool HideHeader { get; set; }

    /// <summary>
    /// 是否隐藏周数。
    /// Hides week numbers.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideWeekNumber")]
    public bool HideWeekNumber { get; set; }

    /// <summary>
    /// 下一页事件。
    /// Next page event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onNext")]
    public EventCallback OnNext { get; set; }

    /// <summary>
    /// 上一页事件。
    /// Previous page event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onPrev")]
    public EventCallback OnPrev { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 头部插槽内容。
    /// Header slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("header")]
    public RenderFragment<VCalendarHeaderSlotContext>? Header { get; set; }

    /// <summary>
    /// 事件插槽内容。
    /// Event slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("event")]
    public RenderFragment<VCalendarEventSlotContext>? EventContent { get; set; }
}
