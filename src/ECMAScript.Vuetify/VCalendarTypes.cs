using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

// Defines VCalendar values, collection carriers, callbacks, and slot contexts.
// 定义 VCalendar 的值域、集合载体、回调与插槽上下文；多分支值优先使用 C# 原生 union。

/// <summary>
/// 日历视图模式（月、周、日）。
/// Calendar view mode (month, week, day).
/// </summary>
[String]
public enum VuetifyCalendarViewMode
{
    /// <summary>
    /// 按月显示；上游取值为 “month”。
    /// </summary>
    [Description("@#month")]
    Month,

    /// <summary>
    /// 按周显示；上游取值为 “week”。
    /// </summary>
    [Description("@#week")]
    Week,

    /// <summary>
    /// 按天显示；上游取值为 “day”。
    /// </summary>
    [Description("@#day")]
    Day
}

/// <summary>
/// 月份中周数的计算方式。
/// Weeks-in-month calculation mode.
/// </summary>
[String]
public enum VuetifyCalendarWeeksInMonth
{
    /// <summary>
    /// 使用静态定位；上游取值为 “static”。
    /// </summary>
    [Description("@#static")]
    Static,

    /// <summary>
    /// 根据当前月份需要的周数决定显示行数；上游取值为 “dynamic”。
    /// </summary>
    [Description("@#dynamic")]
    Dynamic
}

/// <summary>
/// 日历日期值，支持 Date、string 或 Number 类型。
/// Calendar date value supporting Date, string, or Number types.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyCalendarDateValue(Date, string, Number)
{
    /// <summary>
    /// 读取当前值的 Date 分支；不属于该分支时返回 null。
    /// </summary>
    public Date? AsDate => Value is Date value ? value : default(Date?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 将 Date 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(Date value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(string value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(Number value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyCalendarDateValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValue(decimal value)
        => new((Number)value);
}

/// <summary>
/// 日历日期值集合，用于多日期选择。
/// Calendar date value collection for multi-date selection.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyCalendarDateValuesCollectionBuilder), nameof(VuetifyCalendarDateValuesCollectionBuilder.Create))]
public readonly union VuetifyCalendarDateValues(VuetifyCalendarDateValue[]) : IEnumerable<VuetifyCalendarDateValue>
{
    /// <summary>
    /// 读取当前值的 VuetifyCalendarDateValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyCalendarDateValue[]? AsArray
        => Value is VuetifyCalendarDateValue[] value ? value : default(VuetifyCalendarDateValue[]?);

    /// <summary>
    /// 将 VuetifyCalendarDateValue[] 值转换为 VuetifyCalendarDateValues，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValues(VuetifyCalendarDateValue[] values)
        => new(values);

    /// <summary>
    /// 将 Date[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValues(Date[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyCalendarDateValue)value));

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyCalendarDateValue)value));

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyCalendarDateValue)value));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyCalendarDateValue)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarDateValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyCalendarDateValue)value));

    IEnumerator<VuetifyCalendarDateValue> IEnumerable<VuetifyCalendarDateValue>.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarDateValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarDateValue>)this).GetEnumerator();
}


/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyCalendarDateValuesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifyCalendarDateValues Create(ReadOnlySpan<VuetifyCalendarDateValue> values)
        => values.ToArray();
}

/// <summary>
/// 日历允许日期解析委托，用于判断某日期是否可选。
/// Delegate to resolve whether a calendar date is allowed for selection.
/// </summary>
public delegate bool VuetifyCalendarAllowedDateResolver(VuetifyCalendarDateValue date);

/// <summary>
/// 日历允许日期值，支持日期集合或自定义过滤函数。
/// Calendar allowed dates value supporting date collections or custom filter functions.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyCalendarAllowedDatesValue(
    VuetifyCalendarDateValues,
    VuetifyCalendarAllowedDateResolver)
{
    /// <summary>
    /// 读取当前值的 VuetifyCalendarDateValues 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyCalendarDateValues? AsDates
        => Value is VuetifyCalendarDateValues value ? value : default(VuetifyCalendarDateValues?);

    /// <summary>
    /// 读取当前值的 VuetifyCalendarAllowedDateResolver 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyCalendarAllowedDateResolver? AsResolver => Value as VuetifyCalendarAllowedDateResolver;

    /// <summary>
    /// 将 VuetifyCalendarDateValues 值转换为 VuetifyCalendarAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarAllowedDatesValue(VuetifyCalendarDateValues dates)
        => new(dates);

    /// <summary>
    /// 将 VuetifyCalendarAllowedDateResolver 值转换为 VuetifyCalendarAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarAllowedDatesValue(VuetifyCalendarAllowedDateResolver resolver)
        => new(resolver);

    /// <summary>
    /// 将 VuetifyCalendarDateValue[] 值转换为 VuetifyCalendarAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarAllowedDatesValue(VuetifyCalendarDateValue[] values)
        => new((VuetifyCalendarDateValues)values);

    /// <summary>
    /// 将 Date[] 值转换为 VuetifyCalendarAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarAllowedDatesValue(Date[] values)
        => new((VuetifyCalendarDateValues)values);

    /// <summary>
    /// 将 string[] 值转换为 VuetifyCalendarAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarAllowedDatesValue(string[] values)
        => new((VuetifyCalendarDateValues)values);

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyCalendarAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarAllowedDatesValue(Number[] values)
        => new((VuetifyCalendarDateValues)values);

    /// <summary>
    /// 将 int[] 值转换为 VuetifyCalendarAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarAllowedDatesValue(int[] values)
        => new((VuetifyCalendarDateValues)values);

    /// <summary>
    /// 将 double[] 值转换为 VuetifyCalendarAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarAllowedDatesValue(double[] values)
        => new((VuetifyCalendarDateValues)values);
}

/// <summary>
/// 日历事件项，包含标题、起止时间和颜色等属性。
/// Calendar event item with title, start/end time, and color properties.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyCalendarEventItem : VueProps
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }

    /// <summary>
    /// 当前事件或时间区间的起始日期时间。
    /// </summary>
    [Description("@#start")]
    public VuetifyCalendarDateValue? Start { get; init; }

    /// <summary>
    /// 当前事件或时间区间的结束日期时间。
    /// </summary>
    [Description("@#end")]
    public VuetifyCalendarDateValue? End { get; init; }

    /// <summary>
    /// 当前项的主题颜色名或 CSS 颜色值。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 是否将当前事件作为全天事件显示。
    /// </summary>
    [Description("@#allDay")]
    public bool? AllDay { get; init; }
}

/// <summary>
/// 日历事件集合，用于批量传递事件数据。
/// Calendar event collection for bulk event data.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyCalendarEventsCollectionBuilder), nameof(VuetifyCalendarEventsCollectionBuilder.Create))]
public readonly union VuetifyCalendarEvents(VuetifyCalendarEventItem[]) : IEnumerable<VuetifyCalendarEventItem>
{
    /// <summary>
    /// 读取当前值的 VuetifyCalendarEventItem[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyCalendarEventItem[]? AsArray
        => Value is VuetifyCalendarEventItem[] value ? value : default(VuetifyCalendarEventItem[]?);

    /// <summary>
    /// 将 VuetifyCalendarEventItem[] 值转换为 VuetifyCalendarEvents，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarEvents(VuetifyCalendarEventItem[] events)
        => new(events);

    IEnumerator<VuetifyCalendarEventItem> IEnumerable<VuetifyCalendarEventItem>.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarEventItem>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarEventItem>)this).GetEnumerator();
}


/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyCalendarEventsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    public static VuetifyCalendarEvents Create(ReadOnlySpan<VuetifyCalendarEventItem> events)
        => events.ToArray();
}

/// <summary>
/// 日历日对象，包含日期、格式化文本和状态标志。
/// Calendar day object with date, formatted text, and status flags.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyCalendarDay : VueProps
{
    /// <summary>
    /// 当前日历单元格对应的日期。
    /// </summary>
    [Description("@#date")]
    public VuetifyCalendarDateValue? Date { get; init; }

    /// <summary>
    /// 当前日历日期的 ISO 日期字符串。
    /// </summary>
    [Description("@#isoDate")]
    public string? IsoDate { get; init; }

    /// <summary>
    /// 按日历显示规则格式化后的日期文本。
    /// </summary>
    [Description("@#formatted")]
    public string? Formatted { get; init; }

    /// <summary>
    /// 当前日历日期所在的年份。
    /// </summary>
    [Description("@#year")]
    public Number? Year { get; init; }

    /// <summary>
    /// 当前日历日期所在的月份索引，按日期适配器的约定表示。
    /// </summary>
    [Description("@#month")]
    public Number? Month { get; init; }

    /// <summary>
    /// 按当前 locale 格式化的日期文本。
    /// </summary>
    [Description("@#localized")]
    public string? Localized { get; init; }

    /// <summary>
    /// 当前控件是否被禁用。
    /// </summary>
    [Description("@#isDisabled")]
    public bool? IsDisabled { get; init; }

    /// <summary>
    /// 该日期是否为今天。
    /// </summary>
    [Description("@#isToday")]
    public bool? IsToday { get; init; }

    /// <summary>
    /// 该日期是否来自相邻月份，用于月视图首尾补齐。
    /// </summary>
    [Description("@#isAdjacent")]
    public bool? IsAdjacent { get; init; }

    /// <summary>
    /// 当前条目是否已选中。
    /// </summary>
    [Description("@#isSelected")]
    public bool? IsSelected { get; init; }
}

/// <summary>
/// 日历时间间隔，包含标签、起止时间和关联事件。
/// Calendar time interval with label, start/end time, and associated events.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyCalendarInterval : VueProps
{
    /// <summary>
    /// 供当前控件或数据项显示的标签文本。
    /// </summary>
    [Description("@#label")]
    public string? Label { get; init; }

    /// <summary>
    /// 当前事件或时间区间的起始日期时间。
    /// </summary>
    [Description("@#start")]
    public VuetifyCalendarDateValue? Start { get; init; }

    /// <summary>
    /// 当前事件或时间区间的结束日期时间。
    /// </summary>
    [Description("@#end")]
    public VuetifyCalendarDateValue? End { get; init; }

    /// <summary>
    /// 落在当前时间区间内的日历事件。
    /// </summary>
    [Description("@#events")]
    public VuetifyCalendarEvents? Events { get; init; }
}

/// <summary>
/// 日历时间间隔格式化委托，用于自定义间隔标签文本。
/// Delegate to format calendar interval label text.
/// </summary>
public delegate string VuetifyCalendarIntervalFormatter(VuetifyCalendarInterval interval);

/// <summary>
/// 日历时间间隔格式值，支持格式字符串或自定义格式化函数。
/// Calendar interval format value supporting format strings or custom formatter functions.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyCalendarIntervalFormatValue(string, VuetifyCalendarIntervalFormatter)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsFormat => Value as string;

    /// <summary>
    /// 读取当前值的 VuetifyCalendarIntervalFormatter 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyCalendarIntervalFormatter? AsFormatter => Value as VuetifyCalendarIntervalFormatter;

    /// <summary>
    /// 将 string 值转换为 VuetifyCalendarIntervalFormatValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarIntervalFormatValue(string value)
        => new(value);

    /// <summary>
    /// 将 VuetifyCalendarIntervalFormatter 值转换为 VuetifyCalendarIntervalFormatValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarIntervalFormatValue(VuetifyCalendarIntervalFormatter value)
        => new(value);
}

/// <summary>
/// 日历头部插槽上下文，提供标题数据。
/// Calendar header slot context providing title data.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VCalendarHeaderSlotContext
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }
}

/// <summary>
/// 日历事件插槽上下文，提供日、全天标志和事件数据。
/// Calendar event slot context providing day, all-day flag, and event data.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VCalendarEventSlotContext
{
    /// <summary>
    /// 当前事件所在的日历日期及状态。
    /// </summary>
    [Description("@#day")]
    public VuetifyCalendarDay? Day { get; init; }

    /// <summary>
    /// 是否将当前事件作为全天事件显示。
    /// </summary>
    [Description("@#allDay")]
    public bool AllDay { get; init; }

    /// <summary>
    /// 当前渲染的日历事件数据。
    /// </summary>
    [Description("@#event")]
    public VuetifyCalendarEventItem? Event { get; init; }
}