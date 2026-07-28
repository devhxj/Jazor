using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// 日历视图模式（月、周、日）。
/// Calendar view mode (month, week, day).
/// </summary>
[String]
public enum VuetifyCalendarViewMode
{
    [Description("@#month")]
    Month,

    [Description("@#week")]
    Week,

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
    [Description("@#static")]
    Static,

    [Description("@#dynamic")]
    Dynamic
}

/// <summary>
/// 日历日期值，支持 Date、string 或 Number 类型。
/// Calendar date value supporting Date, string, or Number types.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyCalendarDateValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly Date? _date;
    private readonly string? _string;
    private readonly Number? _number;

    public VuetifyCalendarDateValue(Date value)
    {
        _kind = 1;
        _date = value;
        _string = default;
        _number = default;
    }

    public VuetifyCalendarDateValue(string value)
    {
        _kind = 2;
        _date = default;
        _string = value;
        _number = default;
    }

    public VuetifyCalendarDateValue(Number value)
    {
        _kind = 3;
        _date = default;
        _string = default;
        _number = value;
    }

    public Date? AsDate => _kind == 1 ? _date : default;

    public string? AsString => _kind == 2 ? _string : default;

    public Number? AsNumber => _kind == 3 ? _number : default;

    public object? Value => _kind switch
    {
        1 => AsDate,
        2 => AsString,
        3 => AsNumber,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCalendarDateValue From(Date value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCalendarDateValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCalendarDateValue From(Number value);

    public static implicit operator VuetifyCalendarDateValue(Date value)
        => new(value);

    public static implicit operator VuetifyCalendarDateValue(string value)
        => new(value);

    public static implicit operator VuetifyCalendarDateValue(Number value)
        => new(value);

    public static implicit operator VuetifyCalendarDateValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyCalendarDateValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyCalendarDateValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyCalendarDateValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyCalendarDateValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyCalendarDateValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyCalendarDateValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyCalendarDateValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyCalendarDateValue(decimal value)
        => new((Number)value);
}

/// <summary>
/// 日历日期值集合，用于多日期选择。
/// Calendar date value collection for multi-date selection.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyCalendarDateValuesCollectionBuilder), nameof(VuetifyCalendarDateValuesCollectionBuilder.Create))]
public readonly struct VuetifyCalendarDateValues : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyCalendarDateValue>
{
    private readonly VuetifyCalendarDateValue[]? _values;

    public VuetifyCalendarDateValues(VuetifyCalendarDateValue[] values)
    {
        _values = values;
    }

    public VuetifyCalendarDateValue[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCalendarDateValues From(VuetifyCalendarDateValue[] values);

    public static implicit operator VuetifyCalendarDateValues(VuetifyCalendarDateValue[] values)
        => new(values);

    public static implicit operator VuetifyCalendarDateValues(Date[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyCalendarDateValue)value));

    public static implicit operator VuetifyCalendarDateValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyCalendarDateValue)value));

    public static implicit operator VuetifyCalendarDateValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyCalendarDateValue)value));

    public static implicit operator VuetifyCalendarDateValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyCalendarDateValue)value));

    public static implicit operator VuetifyCalendarDateValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VuetifyCalendarDateValue)value));

    IEnumerator<VuetifyCalendarDateValue> IEnumerable<VuetifyCalendarDateValue>.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarDateValue>)(_values ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarDateValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyCalendarDateValuesCollectionBuilder
{
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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyCalendarAllowedDatesValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VuetifyCalendarDateValues? _dates;
    private readonly VuetifyCalendarAllowedDateResolver? _resolver;

    public VuetifyCalendarAllowedDatesValue(VuetifyCalendarDateValues dates)
    {
        _kind = 1;
        _dates = dates;
        _resolver = default;
    }

    public VuetifyCalendarAllowedDatesValue(VuetifyCalendarAllowedDateResolver resolver)
    {
        _kind = 2;
        _dates = default;
        _resolver = resolver;
    }

    public VuetifyCalendarDateValues? AsDates => _kind == 1 ? _dates : default;

    public VuetifyCalendarAllowedDateResolver? AsResolver => _kind == 2 ? _resolver : default;

    public object? Value => _kind switch
    {
        1 => AsDates,
        2 => AsResolver,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCalendarAllowedDatesValue From(VuetifyCalendarDateValues dates);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCalendarAllowedDatesValue From(VuetifyCalendarAllowedDateResolver resolver);

    public static implicit operator VuetifyCalendarAllowedDatesValue(VuetifyCalendarDateValues dates)
        => new(dates);

    public static implicit operator VuetifyCalendarAllowedDatesValue(VuetifyCalendarAllowedDateResolver resolver)
        => new(resolver);

    public static implicit operator VuetifyCalendarAllowedDatesValue(VuetifyCalendarDateValue[] values)
        => new((VuetifyCalendarDateValues)values);

    public static implicit operator VuetifyCalendarAllowedDatesValue(Date[] values)
        => new((VuetifyCalendarDateValues)values);

    public static implicit operator VuetifyCalendarAllowedDatesValue(string[] values)
        => new((VuetifyCalendarDateValues)values);

    public static implicit operator VuetifyCalendarAllowedDatesValue(Number[] values)
        => new((VuetifyCalendarDateValues)values);

    public static implicit operator VuetifyCalendarAllowedDatesValue(int[] values)
        => new((VuetifyCalendarDateValues)values);

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
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#start")]
    public VuetifyCalendarDateValue? Start { get; init; }

    [Description("@#end")]
    public VuetifyCalendarDateValue? End { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#allDay")]
    public bool? AllDay { get; init; }
}

/// <summary>
/// 日历事件集合，用于批量传递事件数据。
/// Calendar event collection for bulk event data.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyCalendarEventsCollectionBuilder), nameof(VuetifyCalendarEventsCollectionBuilder.Create))]
public readonly struct VuetifyCalendarEvents : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyCalendarEventItem>
{
    private readonly VuetifyCalendarEventItem[]? _events;

    public VuetifyCalendarEvents(VuetifyCalendarEventItem[] events)
    {
        _events = events;
    }

    public VuetifyCalendarEventItem[]? AsArray => _events;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCalendarEvents From(VuetifyCalendarEventItem[] events);

    public static implicit operator VuetifyCalendarEvents(VuetifyCalendarEventItem[] events)
        => new(events);

    IEnumerator<VuetifyCalendarEventItem> IEnumerable<VuetifyCalendarEventItem>.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarEventItem>)(_events ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarEventItem>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyCalendarEventsCollectionBuilder
{
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
    [Description("@#date")]
    public VuetifyCalendarDateValue? Date { get; init; }

    [Description("@#isoDate")]
    public string? IsoDate { get; init; }

    [Description("@#formatted")]
    public string? Formatted { get; init; }

    [Description("@#year")]
    public Number? Year { get; init; }

    [Description("@#month")]
    public Number? Month { get; init; }

    [Description("@#localized")]
    public string? Localized { get; init; }

    [Description("@#isDisabled")]
    public bool? IsDisabled { get; init; }

    [Description("@#isToday")]
    public bool? IsToday { get; init; }

    [Description("@#isAdjacent")]
    public bool? IsAdjacent { get; init; }

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
    [Description("@#label")]
    public string? Label { get; init; }

    [Description("@#start")]
    public VuetifyCalendarDateValue? Start { get; init; }

    [Description("@#end")]
    public VuetifyCalendarDateValue? End { get; init; }

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyCalendarIntervalFormatValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _format;
    private readonly VuetifyCalendarIntervalFormatter? _formatter;

    public VuetifyCalendarIntervalFormatValue(string value)
    {
        _kind = 1;
        _format = value;
        _formatter = default;
    }

    public VuetifyCalendarIntervalFormatValue(VuetifyCalendarIntervalFormatter value)
    {
        _kind = 2;
        _format = default;
        _formatter = value;
    }

    public string? AsFormat => _kind == 1 ? _format : default;

    public VuetifyCalendarIntervalFormatter? AsFormatter => _kind == 2 ? _formatter : default;

    public object? Value => _kind switch
    {
        1 => AsFormat,
        2 => AsFormatter,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCalendarIntervalFormatValue From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyCalendarIntervalFormatValue From(VuetifyCalendarIntervalFormatter value);

    public static implicit operator VuetifyCalendarIntervalFormatValue(string value)
        => new(value);

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
    [Description("@#day")]
    public VuetifyCalendarDay? Day { get; init; }

    [Description("@#allDay")]
    public bool AllDay { get; init; }

    [Description("@#event")]
    public VuetifyCalendarEventItem? Event { get; init; }
}
