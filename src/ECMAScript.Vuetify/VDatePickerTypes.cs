using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

// Defines VDatePicker value domains and slot contexts.
// 定义 VDatePicker 的值域与插槽上下文；可擦除的多值域使用原生 union。

/// <summary>
/// Vuetify 日期选择器视图模式。
/// Vuetify date-picker view mode.
/// </summary>
[String]
public enum VuetifyDatePickerViewMode
{
    [Description("@#month")]
    Month,

    [Description("@#months")]
    Months,

    [Description("@#year")]
    Year
}

/// <summary>
/// Vuetify 日期选择器每月周数模式。
/// Vuetify date-picker weeks-in-month mode.
/// </summary>
[String]
public enum VuetifyDatePickerWeeksInMonth
{
    [Description("@#static")]
    Static,

    [Description("@#dynamic")]
    Dynamic
}

/// <summary>
/// Vuetify 日期选择器多选模式。
/// Vuetify date-picker multiple-selection mode.
/// </summary>
[String]
public enum VuetifyDatePickerMultipleMode
{
    [Description("@#range")]
    Range
}

/// <summary>
/// Vuetify 日历星期枚举。
/// Vuetify calendar weekday enumeration.
/// </summary>
public enum VuetifyCalendarWeekday
{
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6
}

/// <summary>
/// Vuetify 日历工作日集合。
/// Vuetify calendar weekdays collection.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyCalendarWeekdaysCollectionBuilder), nameof(VuetifyCalendarWeekdaysCollectionBuilder.Create))]
public readonly union VuetifyCalendarWeekdays(VuetifyCalendarWeekday[]) : IEnumerable<VuetifyCalendarWeekday>
{
    public VuetifyCalendarWeekday[]? AsArray => Value as VuetifyCalendarWeekday[];

    public static implicit operator VuetifyCalendarWeekdays(VuetifyCalendarWeekday[] weekdays)
        => new(weekdays);

    IEnumerator<VuetifyCalendarWeekday> IEnumerable<VuetifyCalendarWeekday>.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarWeekday>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarWeekday>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyCalendarWeekdaysCollectionBuilder
{
    public static VuetifyCalendarWeekdays Create(ReadOnlySpan<VuetifyCalendarWeekday> weekdays)
        => weekdays.ToArray();
}

/// <summary>
/// Vuetify 日期选择器多选值。
/// Vuetify date-picker multiple-selection value.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDatePickerMultipleValue(bool, Number, VuetifyDatePickerMultipleMode, string)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public VuetifyDatePickerMultipleMode? AsMode
        => Value is VuetifyDatePickerMultipleMode value ? value : default(VuetifyDatePickerMultipleMode?);

    public string? AsCustomMode => Value as string;

    public static implicit operator VuetifyDatePickerMultipleValue(bool value)
        => new(value);

    public static implicit operator VuetifyDatePickerMultipleValue(Number value)
        => new(value);

    public static implicit operator VuetifyDatePickerMultipleValue(VuetifyDatePickerMultipleMode value)
        => new(value);

    public static implicit operator VuetifyDatePickerMultipleValue(string value)
        => new(value);

    public static implicit operator VuetifyDatePickerMultipleValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerMultipleValue(decimal value)
        => new((Number)value);
}

/// <summary>
/// Vuetify 日期选择器模型值集合。
/// Vuetify date-picker model value collection.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDatePickerModelValuesCollectionBuilder), nameof(VuetifyDatePickerModelValuesCollectionBuilder.Create))]
public readonly union VuetifyDatePickerModelValues(VueValue[]) : IEnumerable<VueValue>
{
    public VueValue[]? AsArray => Value as VueValue[];

    public static implicit operator VuetifyDatePickerModelValues(VueValue[] values)
        => new(values);

    public static implicit operator VuetifyDatePickerModelValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerModelValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerModelValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerModelValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDatePickerModelValuesCollectionBuilder
{
    public static VuetifyDatePickerModelValues Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

/// <summary>
/// Vuetify 日期选择器模型值。
/// Vuetify date-picker model value.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDatePickerModelValue(Date, string, Number, VuetifyDatePickerModelValues)
{
    public Date? AsDate => Value is Date value ? value : default(Date?);

    public string? AsString => Value as string;

    public Number? AsNumber => Value is Number value ? value : default(Number?);

    public VuetifyDatePickerModelValues? AsValues
        => Value is VuetifyDatePickerModelValues value ? value : default(VuetifyDatePickerModelValues?);

    public static implicit operator VuetifyDatePickerModelValue(Date value)
        => new(value);

    public static implicit operator VuetifyDatePickerModelValue(string value)
        => new(value);

    public static implicit operator VuetifyDatePickerModelValue(Number value)
        => new(value);

    public static implicit operator VuetifyDatePickerModelValue(VuetifyDatePickerModelValues value)
        => new(value);

    public static implicit operator VuetifyDatePickerModelValue(VueValue[] value)
        => new((VuetifyDatePickerModelValues)value);

    public static implicit operator VuetifyDatePickerModelValue(string[] value)
        => new((VuetifyDatePickerModelValues)value);

    public static implicit operator VuetifyDatePickerModelValue(Number[] value)
        => new((VuetifyDatePickerModelValues)value);

    public static implicit operator VuetifyDatePickerModelValue(int[] value)
        => new((VuetifyDatePickerModelValues)value);

    public static implicit operator VuetifyDatePickerModelValue(double[] value)
        => new((VuetifyDatePickerModelValues)value);

    public static implicit operator VuetifyDatePickerModelValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyDatePickerModelValue(decimal value)
        => new((Number)value);
}

public delegate bool VuetifyDatePickerAllowedDateResolver(VueValue? date);

/// <summary>
/// Vuetify 日期选择器允许日期集合。
/// Vuetify date-picker allowed-dates collection.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDatePickerAllowedDatesCollectionBuilder), nameof(VuetifyDatePickerAllowedDatesCollectionBuilder.Create))]
public readonly union VuetifyDatePickerAllowedDates(VueValue[]) : IEnumerable<VueValue>
{
    public VueValue[]? AsArray => Value as VueValue[];

    public static implicit operator VuetifyDatePickerAllowedDates(VueValue[] values)
        => new(values);

    public static implicit operator VuetifyDatePickerAllowedDates(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerAllowedDates(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerAllowedDates(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDatePickerAllowedDates(double[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDatePickerAllowedDatesCollectionBuilder
{
    public static VuetifyDatePickerAllowedDates Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

/// <summary>
/// Vuetify 日期选择器允许日期值。
/// Vuetify date-picker allowed-dates value.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDatePickerAllowedDatesValue(
    VuetifyDatePickerAllowedDates,
    VuetifyDatePickerAllowedDateResolver)
{
    public VuetifyDatePickerAllowedDates? AsDates
        => Value is VuetifyDatePickerAllowedDates value ? value : default(VuetifyDatePickerAllowedDates?);

    public VuetifyDatePickerAllowedDateResolver? AsResolver => Value as VuetifyDatePickerAllowedDateResolver;

    public static implicit operator VuetifyDatePickerAllowedDatesValue(VuetifyDatePickerAllowedDates dates)
        => new(dates);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(VuetifyDatePickerAllowedDateResolver resolver)
        => new(resolver);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(VueValue[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(string[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(Number[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(int[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    public static implicit operator VuetifyDatePickerAllowedDatesValue(double[] values)
        => new((VuetifyDatePickerAllowedDates)values);
}

/// <summary>
/// Vuetify 日期选择器激活值。
/// Vuetify date-picker active value.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDatePickerActiveValue(string, string[])
{
    public string? AsString => Value as string;

    public string[]? AsStrings => Value as string[];

    public static implicit operator VuetifyDatePickerActiveValue(string value)
        => new(value);

    public static implicit operator VuetifyDatePickerActiveValue(string[] value)
        => new(value);
}

/// <summary>
/// Vuetify VDatePicker 标题插槽上下文。
/// Vuetify VDatePicker header slot context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDatePickerHeaderSlotContext
{
    [Description("@#header")]
    public string? Header { get; init; }

    [Description("@#transition")]
    public string? Transition { get; init; }
}
