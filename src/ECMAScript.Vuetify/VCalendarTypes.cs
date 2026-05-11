using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

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

[String]
public enum VuetifyCalendarWeeksInMonth
{
    [Description("@#static")]
    Static,

    [Description("@#dynamic")]
    Dynamic
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyCalendarDateValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly Date? _date;
    private readonly string? _string;
    private readonly Number? _number;

    private VuetifyCalendarDateValue(Date value)
    {
        _kind = 1;
        _date = value;
        _string = default;
        _number = default;
    }

    private VuetifyCalendarDateValue(string value)
    {
        _kind = 2;
        _date = default;
        _string = value;
        _number = default;
    }

    private VuetifyCalendarDateValue(Number value)
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

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyCalendarDateValuesCollectionBuilder), nameof(VuetifyCalendarDateValuesCollectionBuilder.Create))]
public readonly struct VuetifyCalendarDateValues : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyCalendarDateValue>
{
    private readonly VuetifyCalendarDateValue[]? _values;

    private VuetifyCalendarDateValues(VuetifyCalendarDateValue[] values)
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

public delegate bool VuetifyCalendarAllowedDateResolver(VuetifyCalendarDateValue date);

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyCalendarAllowedDatesValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VuetifyCalendarDateValues? _dates;
    private readonly VuetifyCalendarAllowedDateResolver? _resolver;

    private VuetifyCalendarAllowedDatesValue(VuetifyCalendarDateValues dates)
    {
        _kind = 1;
        _dates = dates;
        _resolver = default;
    }

    private VuetifyCalendarAllowedDatesValue(VuetifyCalendarAllowedDateResolver resolver)
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

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyCalendarEventsCollectionBuilder), nameof(VuetifyCalendarEventsCollectionBuilder.Create))]
public readonly struct VuetifyCalendarEvents : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifyCalendarEventItem>
{
    private readonly VuetifyCalendarEventItem[]? _events;

    private VuetifyCalendarEvents(VuetifyCalendarEventItem[] events)
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

public delegate string VuetifyCalendarIntervalFormatter(VuetifyCalendarInterval interval);

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifyCalendarIntervalFormatValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _format;
    private readonly VuetifyCalendarIntervalFormatter? _formatter;

    private VuetifyCalendarIntervalFormatValue(string value)
    {
        _kind = 1;
        _format = value;
        _formatter = default;
    }

    private VuetifyCalendarIntervalFormatValue(VuetifyCalendarIntervalFormatter value)
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

[ECMAScript]
[Description("@#")]
public sealed record VCalendarHeaderSlotContext
{
    [Description("@#title")]
    public string? Title { get; init; }
}

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
