using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 日期选择器创作代理。
/// Vuetify date-picker authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDatePicker")]
public sealed class VDatePicker : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 选中日期的绑定值。
    /// Bound value for the selected date.
    /// </summary>
    [Parameter]
    public VuetifyDatePickerModelValue? ModelValue { get; set; }

    /// <summary>
    /// 选中日期变化时的回调。
    /// Callback when the selected date changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDatePickerModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否允许多选日期。
    /// Whether to allow selecting multiple dates.
    /// </summary>
    [Parameter]
    public VuetifyDatePickerMultipleValue? Multiple { get; set; }

    /// <summary>
    /// 允许选择的最小日期。
    /// Minimum selectable date.
    /// </summary>
    [Parameter]
    public VuetifyDatePickerModelValue? Min { get; set; }

    /// <summary>
    /// 允许选择的最大日期。
    /// Maximum selectable date.
    /// </summary>
    [Parameter]
    public VuetifyDatePickerModelValue? Max { get; set; }

    /// <summary>
    /// 当前显示的年份。
    /// Currently displayed year.
    /// </summary>
    [Parameter]
    public int? Year { get; set; }

    /// <summary>
    /// 年份变化时的回调。
    /// Callback when the displayed year changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> YearChanged { get; set; }

    /// <summary>
    /// 当前显示的月份。
    /// Currently displayed month.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Month { get; set; }

    /// <summary>
    /// 月份变化时的回调。
    /// Callback when the displayed month changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> MonthChanged { get; set; }

    /// <summary>
    /// 选择器的视图模式。
    /// View mode of the picker.
    /// </summary>
    [Parameter]
    public VuetifyDatePickerViewMode? ViewMode { get; set; }

    /// <summary>
    /// 视图模式变化时的回调。
    /// Callback when the view mode changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDatePickerViewMode> ViewModeChanged { get; set; }

    /// <summary>
    /// 当前激活的日期。
    /// Currently active date.
    /// </summary>
    [Parameter]
    public VuetifyDatePickerActiveValue? Active { get; set; }

    /// <summary>
    /// 是否禁用选择器。
    /// Whether to disable the picker.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否显示相邻月份的日期。
    /// Whether to show dates from adjacent months.
    /// </summary>
    [Parameter]
    public bool ShowAdjacentMonths { get; set; }

    /// <summary>
    /// 显示的星期列。
    /// Weekdays to display.
    /// </summary>
    [Parameter]
    public VuetifyCalendarWeekdays? Weekdays { get; set; }

    /// <summary>
    /// 每月显示的周数。
    /// Number of weeks displayed per month.
    /// </summary>
    [Parameter]
    public VuetifyDatePickerWeeksInMonth? WeeksInMonth { get; set; }

    /// <summary>
    /// 每周的第一天。
    /// First day of the week.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? FirstDayOfWeek { get; set; }

    /// <summary>
    /// 允许选择的日期函数或数组。
    /// Function or array of allowed selectable dates.
    /// </summary>
    [Parameter]
    public VuetifyDatePickerAllowedDatesValue? AllowedDates { get; set; }

    /// <summary>
    /// 是否隐藏星期行。
    /// Whether to hide the weekday row.
    /// </summary>
    [Parameter]
    public bool HideWeekdays { get; set; }

    /// <summary>
    /// 是否显示周数。
    /// Whether to show week numbers.
    /// </summary>
    [Parameter]
    public bool ShowWeek { get; set; }

    /// <summary>
    /// 切换月份时的过渡动画。
    /// Transition animation when switching months.
    /// </summary>
    [Parameter]
    public string? Transition { get; set; }

    /// <summary>
    /// 反向切换月份时的过渡动画。
    /// Reverse transition animation when switching months.
    /// </summary>
    [Parameter]
    public string? ReverseTransition { get; set; }

    /// <summary>
    /// 导航控件的高度。
    /// Height of the navigation controls.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? ControlHeight { get; set; }

    /// <summary>
    /// 下一月导航图标。
    /// Icon for next month navigation.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? NextIcon { get; set; }

    /// <summary>
    /// 上一月导航图标。
    /// Icon for previous month navigation.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? PrevIcon { get; set; }

    /// <summary>
    /// 视图模式切换图标。
    /// Icon for switching view mode.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? ModeIcon { get; set; }

    /// <summary>
    /// 选择器的文本内容。
    /// Text content of the picker.
    /// </summary>
    [Parameter]
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
    public string? HeaderColor { get; set; }

    /// <summary>
    /// 组件主题名称。
    /// Component theme name.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 根元素 HTML 标签。
    /// Root element HTML tag.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆角样式。
    /// Border radius style.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    public bool Tile { get; set; }

    /// <summary>
    /// 选择器的定位方式。
    /// Positioning of the picker.
    /// </summary>
    [Parameter]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 选择器的弹出位置。
    /// Popup location of the picker.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 阴影高度级别。
    /// Elevation level.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件高度。
    /// Component height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件最大高度。
    /// Maximum height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件最大宽度。
    /// Maximum width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 组件最小高度。
    /// Minimum height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件最小宽度。
    /// Minimum width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件宽度。
    /// Component width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 边框样式。
    /// Border style.
    /// </summary>
    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 组件主题色。
    /// Component theme color.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 组件背景色。
    /// Component background color.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 是否显示分隔线。
    /// Whether to show dividers.
    /// </summary>
    [Parameter]
    public bool Divided { get; set; }

    /// <summary>
    /// 是否使用横向布局。
    /// Whether to use landscape layout.
    /// </summary>
    [Parameter]
    public bool Landscape { get; set; }

    /// <summary>
    /// 选择器标题文本。
    /// Picker title text.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// 是否隐藏头部。
    /// Whether to hide the header.
    /// </summary>
    [Parameter]
    public bool HideHeader { get; set; }

    /// <summary>
    /// 附加到组件的额外 HTML 属性。
    /// Additional HTML attributes attached to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认子内容插槽。
    /// Default child content slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 操作区域插槽内容。
    /// Slot content for the actions area.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// 头部插槽内容。
    /// Slot content for the header area.
    /// </summary>
    [Parameter]
    public RenderFragment<VDatePickerHeaderSlotContext>? HeaderContent { get; set; }

    /// <summary>
    /// 标题插槽内容。
    /// Slot content for the title area.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }
}
