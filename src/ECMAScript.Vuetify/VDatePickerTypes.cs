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
    /// <summary>
    /// 按月显示；上游取值为 “month”。
    /// </summary>
    [Description("@#month")]
    Month,

    /// <summary>
    /// 选择多个月份；上游取值为 “months”。
    /// </summary>
    [Description("@#months")]
    Months,

    /// <summary>
    /// 按年显示；上游取值为 “year”。
    /// </summary>
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
/// Vuetify 日期选择器多选模式。
/// Vuetify date-picker multiple-selection mode.
/// </summary>
[String]
public enum VuetifyDatePickerMultipleMode
{
    /// <summary>
    /// 选择日期范围；上游取值为 “range”。
    /// </summary>
    [Description("@#range")]
    Range
}

/// <summary>
/// Vuetify 日历星期枚举。
/// Vuetify calendar weekday enumeration.
/// </summary>
public enum VuetifyCalendarWeekday
{
    /// <summary>
    /// 星期日。
    /// </summary>
    Sunday = 0,
    /// <summary>
    /// 星期一。
    /// </summary>
    Monday = 1,
    /// <summary>
    /// 星期二。
    /// </summary>
    Tuesday = 2,
    /// <summary>
    /// 星期三。
    /// </summary>
    Wednesday = 3,
    /// <summary>
    /// 星期四。
    /// </summary>
    Thursday = 4,
    /// <summary>
    /// 星期五。
    /// </summary>
    Friday = 5,
    /// <summary>
    /// 星期六。
    /// </summary>
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
    /// <summary>
    /// 读取当前值的 VuetifyCalendarWeekday[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyCalendarWeekday[]? AsArray => Value as VuetifyCalendarWeekday[];

    /// <summary>
    /// 将 VuetifyCalendarWeekday[] 值转换为 VuetifyCalendarWeekdays，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyCalendarWeekdays(VuetifyCalendarWeekday[] weekdays)
        => new(weekdays);

    IEnumerator<VuetifyCalendarWeekday> IEnumerable<VuetifyCalendarWeekday>.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarWeekday>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyCalendarWeekday>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyCalendarWeekdaysCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
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
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 VuetifyDatePickerMultipleMode 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDatePickerMultipleMode? AsMode
        => Value is VuetifyDatePickerMultipleMode value ? value : default(VuetifyDatePickerMultipleMode?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsCustomMode => Value as string;

    /// <summary>
    /// 将 bool 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(bool value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(Number value)
        => new(value);

    /// <summary>
    /// 将 VuetifyDatePickerMultipleMode 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(VuetifyDatePickerMultipleMode value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(string value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerMultipleValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyDatePickerMultipleValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
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
    /// <summary>
    /// 读取当前值的 VueValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VueValue[]? AsArray => Value as VueValue[];

    /// <summary>
    /// 将 VueValue[] 值转换为 VuetifyDatePickerModelValues，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValues(VueValue[] values)
        => new(values);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDatePickerModelValuesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
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
    /// 读取当前值的 VuetifyDatePickerModelValues 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDatePickerModelValues? AsValues
        => Value is VuetifyDatePickerModelValues value ? value : default(VuetifyDatePickerModelValues?);

    /// <summary>
    /// 将 Date 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(Date value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(string value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(Number value)
        => new(value);

    /// <summary>
    /// 将 VuetifyDatePickerModelValues 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(VuetifyDatePickerModelValues value)
        => new(value);

    /// <summary>
    /// 将 VueValue[] 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(VueValue[] value)
        => new((VuetifyDatePickerModelValues)value);

    /// <summary>
    /// 将 string[] 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(string[] value)
        => new((VuetifyDatePickerModelValues)value);

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(Number[] value)
        => new((VuetifyDatePickerModelValues)value);

    /// <summary>
    /// 将 int[] 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(int[] value)
        => new((VuetifyDatePickerModelValues)value);

    /// <summary>
    /// 将 double[] 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(double[] value)
        => new((VuetifyDatePickerModelValues)value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyDatePickerModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerModelValue(decimal value)
        => new((Number)value);
}

/// <summary>
/// 用于 VuetifyDatePickerAllowedDatesValue.AsResolver 的回调签名。
/// 读取当前值的 VuetifyDatePickerAllowedDateResolver 分支；不属于该分支时返回 null。
/// </summary>
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
    /// <summary>
    /// 读取当前值的 VueValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VueValue[]? AsArray => Value as VueValue[];

    /// <summary>
    /// 将 VueValue[] 值转换为 VuetifyDatePickerAllowedDates，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDates(VueValue[] values)
        => new(values);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDates(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDates(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDates(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDates(double[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDatePickerAllowedDatesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
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
    /// <summary>
    /// 读取当前值的 VuetifyDatePickerAllowedDates 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDatePickerAllowedDates? AsDates
        => Value is VuetifyDatePickerAllowedDates value ? value : default(VuetifyDatePickerAllowedDates?);

    /// <summary>
    /// 读取当前值的 VuetifyDatePickerAllowedDateResolver 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDatePickerAllowedDateResolver? AsResolver => Value as VuetifyDatePickerAllowedDateResolver;

    /// <summary>
    /// 将 VuetifyDatePickerAllowedDates 值转换为 VuetifyDatePickerAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDatesValue(VuetifyDatePickerAllowedDates dates)
        => new(dates);

    /// <summary>
    /// 将 VuetifyDatePickerAllowedDateResolver 值转换为 VuetifyDatePickerAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDatesValue(VuetifyDatePickerAllowedDateResolver resolver)
        => new(resolver);

    /// <summary>
    /// 将 VueValue[] 值转换为 VuetifyDatePickerAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDatesValue(VueValue[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    /// <summary>
    /// 将 string[] 值转换为 VuetifyDatePickerAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDatesValue(string[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyDatePickerAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDatesValue(Number[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    /// <summary>
    /// 将 int[] 值转换为 VuetifyDatePickerAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerAllowedDatesValue(int[] values)
        => new((VuetifyDatePickerAllowedDates)values);

    /// <summary>
    /// 将 double[] 值转换为 VuetifyDatePickerAllowedDatesValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
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
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 string[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string[]? AsStrings => Value as string[];

    /// <summary>
    /// 将 string 值转换为 VuetifyDatePickerActiveValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDatePickerActiveValue(string value)
        => new(value);

    /// <summary>
    /// 将 string[] 值转换为 VuetifyDatePickerActiveValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
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
    /// <summary>
    /// 日期选择器头部的显示文本。
    /// </summary>
    [Description("@#header")]
    public string? Header { get; init; }

    /// <summary>
    /// 当前日期选择器头部使用的过渡名称。
    /// </summary>
    [Description("@#transition")]
    public string? Transition { get; init; }
}