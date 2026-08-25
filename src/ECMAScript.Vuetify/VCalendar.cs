using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify Labs 日历创作代理。
/// Vuetify labs calendar authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VCalendar")]
public sealed class VCalendar : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 日历的当前日期值。
    /// Current date value of the calendar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyCalendarDateValues? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化时触发的事件。
    /// Event fired when model value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyCalendarDateValues?> ModelValueChanged { get; set; }

    /// <summary>
    /// 下一页图标。
    /// Next page icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("nextIcon")]
    public VuetifyIconValue? NextIcon { get; set; }

    /// <summary>
    /// 上一页图标。
    /// Previous page icon.
    /// </summary>
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
    [Parameter]
    [ECMAScriptName("dayIndex")]
    public Number? DayIndex { get; set; }

    /// <summary>
    /// 事件列表。
    /// Events list.
    /// </summary>
    [Parameter]
    [ECMAScriptName("events")]
    public VuetifyCalendarEvents? Events { get; set; }

    /// <summary>
    /// 时间间隔的分割数。
    /// Number of divisions per interval.
    /// </summary>
    [Parameter]
    [ECMAScriptName("intervalDivisions")]
    public Number? IntervalDivisions { get; set; }

    /// <summary>
    /// 时间间隔的持续时间（分钟）。
    /// Interval duration in minutes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("intervalDuration")]
    public Number? IntervalDuration { get; set; }

    /// <summary>
    /// 时间间隔的像素高度。
    /// Interval height in pixels.
    /// </summary>
    [Parameter]
    [ECMAScriptName("intervalHeight")]
    public Number? IntervalHeight { get; set; }

    /// <summary>
    /// 时间间隔的格式化方式。
    /// Interval format.
    /// </summary>
    [Parameter]
    [ECMAScriptName("intervalFormat")]
    public VuetifyCalendarIntervalFormatValue? IntervalFormat { get; set; }

    /// <summary>
    /// 日开始的时间间隔索引。
    /// Interval start index for the day.
    /// </summary>
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
    [Parameter]
    [ECMAScriptName("intervals")]
    public Number? Intervals { get; set; }

    /// <summary>
    /// 允许选择的日期。
    /// Allowed dates for selection.
    /// </summary>
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
    [Parameter]
    [ECMAScriptName("displayValue")]
    public VuetifyCalendarDateValue? DisplayValue { get; set; }

    /// <summary>
    /// 显示的月份。
    /// Month to display.
    /// </summary>
    [Parameter]
    [ECMAScriptName("month")]
    public VueStringNumberValue? Month { get; set; }

    /// <summary>
    /// 最大日期值。
    /// Maximum date value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("max")]
    public VuetifyCalendarDateValue? Max { get; set; }

    /// <summary>
    /// 最小日期值。
    /// Minimum date value.
    /// </summary>
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
    [Parameter]
    [ECMAScriptName("year")]
    public VueStringNumberValue? Year { get; set; }

    /// <summary>
    /// 一周中显示的星期。
    /// Weekdays to display.
    /// </summary>
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
