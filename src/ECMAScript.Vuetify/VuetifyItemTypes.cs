using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 选择器项目列表的擦除值联合类型。
/// Erased value union for Vuetify select item collections.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySelectItemsCollectionBuilder), nameof(VuetifySelectItemsCollectionBuilder.Create))]
public readonly union VuetifySelectItems(VuetifySelectItemValue[]) : IEnumerable<VuetifySelectItemValue>
{
    /// <summary>
    /// 读取当前值的 VuetifySelectItemValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySelectItemValue[]? AsArray
        => Value is VuetifySelectItemValue[] value ? value : default(VuetifySelectItemValue[]?);

    /// <summary>
    /// 将 VuetifySelectItemValue[] 值转换为 VuetifySelectItems，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItems(VuetifySelectItemValue[] items)
        => new(items);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySelectItemValue)item));

    IEnumerator<VuetifySelectItemValue> IEnumerable<VuetifySelectItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifySelectItemValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySelectItemValue>)this).GetEnumerator();
}


/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySelectItemsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    public static VuetifySelectItems Create(ReadOnlySpan<VuetifySelectItemValue> items)
        => items.ToArray();
}

/// <summary>
/// C# 联合参数，允许 VuetifySelectModelValue[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySelectModelValuesCollectionBuilder), nameof(VuetifySelectModelValuesCollectionBuilder.Create))]
public readonly union VuetifySelectModelValues(VuetifySelectModelValue[]) : IEnumerable<VuetifySelectModelValue>
{
    /// <summary>
    /// 读取当前值的 VuetifySelectModelValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySelectModelValue[]? AsArray
        => Value is VuetifySelectModelValue[] value ? value : default(VuetifySelectModelValue[]?);

    /// <summary>
    /// 将 VuetifySelectModelValue[] 值转换为 VuetifySelectModelValues，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValues(VuetifySelectModelValue[] values)
        => new(values);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    /// <summary>
    /// 将 bool[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValues(bool[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    /// <summary>
    /// 将 Symbol[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValues(Symbol[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    /// <summary>
    /// 将 VueProps[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValues(VueProps[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    /// <summary>
    /// 将 VueDictionary[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValues(VueDictionary[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    IEnumerator<VuetifySelectModelValue> IEnumerable<VuetifySelectModelValue>.GetEnumerator()
        => ((IEnumerable<VuetifySelectModelValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySelectModelValue>)this).GetEnumerator();
}


/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySelectModelValuesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifySelectModelValues Create(ReadOnlySpan<VuetifySelectModelValue> values)
        => values.ToArray();
}

/// <summary>
/// 用于 VAutocomplete.SelectedValue、VAutocomplete.SelectedValueChanged、VCombobox.SelectedValue、VCombobox.SelectedValueChanged、VSelect.SelectedValue、VSelect.SelectedValueChanged 的参数类型。
/// 选中的值。
/// Selected value.
/// 选中值变化时触发的事件。
/// Event fired when selected value changes.
/// 选中项绑定值。
/// The bound selected value.
/// 选中项变更回调。
/// Callback invoked when the selected value changes.
/// 绑定到 v-model 的选中项值。
/// The selected item value bound to v-model.
/// 选中项值变更时触发的回调。
/// Callback invoked when the selected item value changes.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifySelectModelValue(
    string,
    Number,
    bool,
    Symbol,
    VueProps,
    VuetifySelectModelValues)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 Symbol 分支；不属于该分支时返回 null。
    /// </summary>
    public Symbol? AsSymbol
        => Value is Symbol value ? value : default(Symbol?);

    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsObject
        => Value is VueProps value ? value : default(VueProps?);

    /// <summary>
    /// 读取当前值的 VuetifySelectModelValues 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySelectModelValues? AsValues
        => Value is VuetifySelectModelValues value ? value : default(VuetifySelectModelValues?);

    /// <summary>
    /// 将 string 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(string value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(Number value)
        => new(value);

    /// <summary>
    /// 将 bool 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(bool value)
        => new(value);

    /// <summary>
    /// 将 Symbol 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(Symbol value)
        => new(value);

    /// <summary>
    /// 将 VueProps 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(VueProps value)
        => new(value);

    /// <summary>
    /// 将 VueDictionary 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(VueDictionary value)
        => new(value);

    /// <summary>
    /// 将 VuetifySelectModelValues 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(VuetifySelectModelValues value)
        => new(value);

    /// <summary>
    /// 将 VuetifySelectModelValue[] 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(VuetifySelectModelValue[] value)
        => new((VuetifySelectModelValues)value);

    /// <summary>
    /// 将 string[] 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(string[] value)
        => new((VuetifySelectModelValues)value);

    /// <summary>
    /// 将 Number[] 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(Number[] value)
        => new((VuetifySelectModelValues)value);

    /// <summary>
    /// 将 bool[] 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(bool[] value)
        => new((VuetifySelectModelValues)value);

    /// <summary>
    /// 将 Symbol[] 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(Symbol[] value)
        => new((VuetifySelectModelValues)value);

    /// <summary>
    /// 将 VueProps[] 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(VueProps[] value)
        => new((VuetifySelectModelValues)value);

    /// <summary>
    /// 将 VueDictionary[] 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(VueDictionary[] value)
        => new((VuetifySelectModelValues)value);

    /// <summary>
    /// 将 byte 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifySelectModelValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectModelValue(decimal value)
        => new((Number)value);
}


/// <summary>
/// C# 联合参数，允许 string, VuetifySelectItem, Number, bool, VueProps。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifySelectItemValue(
    string,
    VuetifySelectItem,
    Number,
    bool,
    VueProps)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 VuetifySelectItem 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySelectItem? AsItem
        => Value is VuetifySelectItem value ? value : default(VuetifySelectItem?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsObject
        => Value is VueProps value ? value : default(VueProps?);

    /// <summary>
    /// 将 string 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(string value)
        => new(value);

    /// <summary>
    /// 将 VuetifySelectItem 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(VuetifySelectItem value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(Number value)
        => new(value);

    /// <summary>
    /// 将 bool 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VueProps 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(VueProps value)
        => new(value);

    /// <summary>
    /// 将 VueDictionary 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(VueDictionary value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifySelectItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemValue(decimal value)
        => new((Number)value);
}


/// <summary>
/// 选择器的结构化选项，包含显示文本、模型值、子项与附加属性。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifySelectItem
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public VueValue? Value { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VuetifySelectItemPropsValue? Props { get; init; }

    /// <summary>
    /// 当前条目的直接子项，形成嵌套分组或树形结构。
    /// </summary>
    [Description("@#children")]
    public VuetifySelectItemValue[]? Children { get; init; }

    /// <summary>
    /// 调用方提供的原始数据，供自定义渲染读取业务字段。
    /// </summary>
    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

/// <summary>
/// 用于 VDataIterator.ItemValue、VDataIterator.ItemSelectable、VDataTable.ItemValue、VDataTable.ItemSelectable、VList.ItemTitle、VList.ItemValue、VList.ItemChildren、VSelectLikeComponentBase.ItemTitle、VSelectLikeComponentBase.ItemValue、VSelectLikeComponentBase.ItemChildren、VTreeview.ItemTitle、VTreeview.ItemValue、VTreeview.ItemChildren、VVirtualScroll.ItemKey 的参数类型。
/// Property on supplied `items` that contains its value.
/// Property on supplied `items` that contains the boolean value indicating if the item is selectable.
/// Property on supplied `items` that indicates whether the item is selectable.
/// Property on supplied `items` that contains its title.
/// Property on supplied `items` that contains its children.
/// 指定选项对象中用作标题的字段名。
/// The property name used as the item title.
/// 指定选项对象中用作值的字段名。
/// The property name used as the item value.
/// 指定选项对象中用作子级的字段名。
/// The property name used as the item children.
/// Should point to a property with a unique value for each item, if not set then item index will be used as a key which may result in unnecessary re-renders.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifySelectItemKey(
    string,
    string[],
    VuetifySelectItemKeySelector,
    bool)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 string[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string[]? AsPath
        => Value is string[] value ? value : default(string[]?);

    /// <summary>
    /// 读取当前值的 VuetifySelectItemKeySelector 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySelectItemKeySelector? AsSelector
        => Value is VuetifySelectItemKeySelector value ? value : default(VuetifySelectItemKeySelector?);

    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 将 string 值转换为 VuetifySelectItemKey，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemKey(string value)
        => new(value);

    /// <summary>
    /// 将 string[] 值转换为 VuetifySelectItemKey，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemKey(string[] value)
        => new(value);

    /// <summary>
    /// 将 VuetifySelectItemKeySelector 值转换为 VuetifySelectItemKey，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemKey(VuetifySelectItemKeySelector value)
        => new(value);

    /// <summary>
    /// 将 bool 值转换为 VuetifySelectItemKey，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemKey(bool value)
        => new(value);
}


/// <summary>
/// 用于 VuetifySelectItemKey.AsSelector 的回调签名。
/// 读取当前值的 VuetifySelectItemKeySelector 分支；不属于该分支时返回 null。
/// </summary>
public delegate VueValue? VuetifySelectItemKeySelector(VueValue item, string fallback);

/// <summary>
/// 用于 VList.ItemProps、VSelectLikeComponentBase.ItemProps、VTreeview.ItemProps 的参数类型。
/// Props object that will be applied to each item component. `true` will treat the original object as raw props and pass it directly to the component.
/// 指定传递给每个选项的额外属性。
/// The selector for additional props passed to each item.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifySelectItemPropsSelector(
    string,
    string[],
    VuetifySelectItemPropsCallback,
    bool)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 string[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string[]? AsPath
        => Value is string[] value ? value : default(string[]?);

    /// <summary>
    /// 读取当前值的 VuetifySelectItemPropsCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySelectItemPropsCallback? AsCallback
        => Value is VuetifySelectItemPropsCallback value ? value : default(VuetifySelectItemPropsCallback?);

    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 将 string 值转换为 VuetifySelectItemPropsSelector，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemPropsSelector(string value)
        => new(value);

    /// <summary>
    /// 将 string[] 值转换为 VuetifySelectItemPropsSelector，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemPropsSelector(string[] value)
        => new(value);

    /// <summary>
    /// 将 VuetifySelectItemPropsCallback 值转换为 VuetifySelectItemPropsSelector，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemPropsSelector(VuetifySelectItemPropsCallback value)
        => new(value);

    /// <summary>
    /// 将 bool 值转换为 VuetifySelectItemPropsSelector，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemPropsSelector(bool value)
        => new(value);
}


/// <summary>
/// 用于 VuetifySelectItemPropsSelector.AsCallback 的回调签名。
/// 读取当前值的 VuetifySelectItemPropsCallback 分支；不属于该分支时返回 null。
/// </summary>
public delegate VuetifyItemProps? VuetifySelectItemPropsCallback(VueValue item);

/// <summary>
/// 用于 VSelectLikeComponentBase.ValueComparator、VTreeview.ValueComparator 的回调签名。
/// 用于比较选项值的自定义比较器。
/// The custom comparator for comparing item values.
/// Apply a custom comparison algorithm to compare **model-value** and values contains in the **items** prop.
/// </summary>
public delegate bool VuetifySelectValueComparator(VueValue? first, VueValue? second);

/// <summary>
/// 用于 VAutocomplete.CustomFilter、VCombobox.CustomFilter、VDataIterator.CustomFilter、VTreeview.CustomFilter 的回调签名。
/// Function used to filter items, called for each filterable key on each item in the list. The first argument is the filterable value from the item, the second is the search term, and the third is the internal item object. The function should return true if the item should be included in the filtered list, or the index of the match in the value if it should be included with the result highlighted.
/// Function to filter items.
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="query">用户输入的过滤查询文本。</param>
/// <param name="item">正在检查的完整项目及其原始数据。</param>
public delegate VuetifyFilterMatch VuetifyFilterFunction(string? value, string? query, VuetifyListItem? item = default);

/// <summary>
/// 用于 VAutocomplete.CustomKeyFilter、VCombobox.CustomKeyFilter、VDataIterator.CustomKeyFilter、VTreeview.CustomKeyFilter 的参数类型。
/// Function used on specific keys within the item object. `customFilter` is skipped for columns with `customKeyFilter` specified.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyFilterKeyFunctions : VueDictionary<VuetifyFilterFunction>;

/// <summary>
/// 用于 VAutocomplete.FilterMode、VCombobox.FilterMode、VDataIterator.FilterMode、VTreeview.FilterMode 的可选取值。
/// Controls how the results of `customFilter` and `customKeyFilter` are combined. All modes only apply `customFilter` to columns not specified in `customKeyFilter`.
///
/// - **some**: There is at least one match from either the custom filter or the custom key filter.
/// - **every**: All columns match either the custom filter or the custom key filter.
/// - **union**: There is at least one match from the custom filter, or all columns match the custom key filters.
/// - **intersection**: There is at least one match from the custom filter, and all columns match the custom key filters.
/// </summary>
[String]
public enum VuetifyFilterMode
{
    /// <summary>
    /// 至少一个字段匹配时保留条目；上游取值为 “some”。
    /// </summary>
    [Description("@#some")]
    Some,

    /// <summary>
    /// 所有参与过滤的字段均匹配时保留条目；上游取值为 “every”。
    /// </summary>
    [Description("@#every")]
    Every,

    /// <summary>
    /// 常规过滤结果与自定义过滤结果满足其中一组即可；上游取值为 “union”。
    /// </summary>
    [Description("@#union")]
    Union,

    /// <summary>
    /// 常规过滤与自定义过滤结果均需满足；上游取值为 “intersection”。
    /// </summary>
    [Description("@#intersection")]
    Intersection
}

/// <summary>
/// 用于 VAutocomplete.FilterKeys、VCombobox.FilterKeys、VDataIterator.FilterKeys、VTreeview.FilterKeys 的参数类型。
/// Array of specific keys to filter on the item.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyFilterKeys(string, string[])
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 string[] 分支；不属于该分支时返回 null。
    /// </summary>
    public string[]? AsStrings
        => Value is string[] value ? value : default(string[]?);

    /// <summary>
    /// 将 string 值转换为 VuetifyFilterKeys，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterKeys(string value)
        => new(value);

    /// <summary>
    /// 将 string[] 值转换为 VuetifyFilterKeys，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterKeys(string[] value)
        => new(value);
}


/// <summary>
/// C# 联合参数，允许 bool, Number, Number[], Number[][]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyFilterMatch(
    bool,
    Number,
    Number[],
    Number[][])
{
    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 Number[] 分支；不属于该分支时返回 null。
    /// </summary>
    public Number[]? AsRange
        => Value is Number[] value ? value : default(Number[]?);

    /// <summary>
    /// 读取当前值的 Number[][] 分支；不属于该分支时返回 null。
    /// </summary>
    public Number[][]? AsRanges
        => Value is Number[][] value ? value : default(Number[][]?);

    /// <summary>
    /// 构造由 start 和 end 两个位置界定的匹配范围，供过滤结果高亮使用。
    /// </summary>
    [ECMAScriptInline("[__arg1, __arg2]")]
    public extern static VuetifyFilterMatch Range(Number start, Number end);

    /// <summary>
    /// 将 bool 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(bool value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(Number value)
        => new(value);

    /// <summary>
    /// 将 Number[] 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(Number[] value)
        => new(value);

    /// <summary>
    /// 将 Number[][] 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(Number[][] value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyFilterMatch，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyFilterMatch(decimal value)
        => new((Number)value);
}


/// <summary>
/// 用于 VSelectItemSlotContext.Item、VSelectChipSlotContext.Item、VSelectSelectionSlotContext.Item、VuetifyListItem.Children 的参数类型。
/// 当前渲染或操作的数据项。
/// 当前条目的直接子项，形成嵌套分组或树形结构。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyListItem
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public VueValue? Value { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VueProps? Props { get; init; }

    /// <summary>
    /// 当前条目的直接子项，形成嵌套分组或树形结构。
    /// </summary>
    [Description("@#children")]
    public VuetifyListItem[]? Children { get; init; }

    /// <summary>
    /// 调用方提供的原始数据，供自定义渲染读取业务字段。
    /// </summary>
    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

/// <summary>
/// C# 联合参数，允许 VuetifyItemProps, bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifySelectItemPropsValue(VuetifyItemProps, bool)
{
    /// <summary>
    /// 读取当前值的 VuetifyItemProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyItemProps? AsProps
        => Value is VuetifyItemProps value ? value : default(VuetifyItemProps?);

    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 将 VuetifyItemProps 值转换为 VuetifySelectItemPropsValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemPropsValue(VuetifyItemProps value)
        => new(value);

    /// <summary>
    /// 将 bool 值转换为 VuetifySelectItemPropsValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySelectItemPropsValue(bool value)
        => new(value);
}


/// <summary>
/// 用于 VuetifyTreeviewInternalItem.Props、VTreeviewItemSlotContext.Props、VTreeviewStructuralItemSlotContext.Props 的参数类型。
/// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifyItemProps : IEnumerable
{
    /// <summary>
    /// 按属性名读取或写入传给条目组件的附加属性。
    /// </summary>
    public extern VueValue? this[string key] { get; set; }

    /// <summary>
    /// 按字段名写入当前对象的业务值，支持 C# 集合初始化器；同名字段使用新值覆盖。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <param name="key">字段名称；按该名称写入当前数据项的属性。</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern void Add(string key, VueValue value);

    extern IEnumerator IEnumerable.GetEnumerator();
}

/// <summary>
/// 用于 VBreadcrumbs.Items 的参数类型。
/// An array of strings or objects used for automatically generating children components.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyBreadcrumbItemsCollectionBuilder), nameof(VuetifyBreadcrumbItemsCollectionBuilder.Create))]
public readonly union VuetifyBreadcrumbItems(VuetifyBreadcrumbItemValue[]) : IEnumerable<VuetifyBreadcrumbItemValue>
{
    /// <summary>
    /// 读取当前值的 VuetifyBreadcrumbItemValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyBreadcrumbItemValue[]? AsArray
        => Value is VuetifyBreadcrumbItemValue[] value ? value : default(VuetifyBreadcrumbItemValue[]?);

    /// <summary>
    /// 将 VuetifyBreadcrumbItemValue[] 值转换为 VuetifyBreadcrumbItems，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItems(VuetifyBreadcrumbItemValue[] items)
        => new(items);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyBreadcrumbItemValue)item));

    IEnumerator<VuetifyBreadcrumbItemValue> IEnumerable<VuetifyBreadcrumbItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifyBreadcrumbItemValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyBreadcrumbItemValue>)this).GetEnumerator();
}


/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyBreadcrumbItemsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    public static VuetifyBreadcrumbItems Create(ReadOnlySpan<VuetifyBreadcrumbItemValue> items)
        => items.ToArray();
}

/// <summary>
/// C# 联合参数，允许 string, VuetifyBreadcrumbItem, Number。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyBreadcrumbItemValue(
    string,
    VuetifyBreadcrumbItem,
    Number)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString
        => Value is string value ? value : default(string?);

    /// <summary>
    /// 读取当前值的 VuetifyBreadcrumbItem 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyBreadcrumbItem? AsItem
        => Value is VuetifyBreadcrumbItem value ? value : default(VuetifyBreadcrumbItem?);

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 将 string 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(string value)
        => new(value);

    /// <summary>
    /// 将 VuetifyBreadcrumbItem 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(VuetifyBreadcrumbItem value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(Number value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyBreadcrumbItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyBreadcrumbItemValue(decimal value)
        => new((Number)value);
}


/// <summary>
/// 面包屑条目的显示文本、链接目标和禁用状态。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifyBreadcrumbItem
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }

    /// <summary>
    /// 是否禁止此项的用户交互。
    /// </summary>
    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    /// <summary>
    /// 点击面包屑时打开的链接 URL。
    /// </summary>
    [Description("@#href")]
    public string? Href { get; init; }

    /// <summary>
    /// Vue Router 导航的目标位置。
    /// </summary>
    [Description("@#to")]
    public string? To { get; init; }

    /// <summary>
    /// 导航时替换当前历史记录，而不是新增记录。
    /// </summary>
    [Description("@#replace")]
    public bool? Replace { get; init; }

    /// <summary>
    /// 要求精确匹配路由后才应用激活状态。
    /// </summary>
    [Description("@#exact")]
    public bool? Exact { get; init; }
}

/// <summary>
/// 用于 VDataTable.Headers 的参数类型。
/// An array of objects that each describe a header column.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableHeadersCollectionBuilder), nameof(VuetifyDataTableHeadersCollectionBuilder.Create))]
public readonly union VuetifyDataTableHeaders(VuetifyDataTableHeader[]) : IEnumerable<VuetifyDataTableHeader>
{
    /// <summary>
    /// 读取当前值的 VuetifyDataTableHeader[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDataTableHeader[]? AsArray
        => Value is VuetifyDataTableHeader[] value ? value : default(VuetifyDataTableHeader[]?);

    /// <summary>
    /// 将 VuetifyDataTableHeader[] 值转换为 VuetifyDataTableHeaders，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableHeaders(VuetifyDataTableHeader[] headers)
        => new(headers);

    IEnumerator<VuetifyDataTableHeader> IEnumerable<VuetifyDataTableHeader>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableHeader>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableHeader>)this).GetEnumerator();
}


/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableHeadersCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    public static VuetifyDataTableHeaders Create(ReadOnlySpan<VuetifyDataTableHeader> headers)
        => headers.ToArray();
}

/// <summary>
/// 用于 VDataTableSlotContext.Headers、VDataTableSlotContext.Columns、VDataTableHeadersSlotContext.Headers、VDataTableHeadersSlotContext.Columns、VDataTableHeaderCellSlotContext.Column、VDataTableItemSlotContext.Columns、VDataTableGroupHeaderSlotContext.Columns、VuetifyDataTableCellPropsContext.Column、VuetifyDataTableHeader.Children 的参数类型。
/// 按表头层级组织的列定义，每一层对应一行表头。
/// 当前可渲染的叶子列定义，顺序与表格显示列一致。
/// 当前单元格对应的列定义。
/// 当前条目的直接子项，形成嵌套分组或树形结构。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableHeader
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }

    /// <summary>
    /// 列或排序字段的标识，应与条目中的对应数据字段匹配。
    /// </summary>
    [Description("@#key")]
    public string? Key { get; init; }

    /// <summary>
    /// 从行数据中提取此列值的字段键、路径或函数。
    /// </summary>
    [Description("@#value")]
    public VuetifySelectItemKey? Value { get; init; }

    /// <summary>
    /// 是否允许按此列排序。
    /// </summary>
    [Description("@#sortable")]
    public bool? Sortable { get; init; }

    /// <summary>
    /// 此列单元格内容的对齐方式。
    /// </summary>
    [Description("@#align")]
    public VuetifyDataTableHeaderAlign? Align { get; init; }

    /// <summary>
    /// 此列的显示宽度，可使用像素数值或 CSS 长度。
    /// </summary>
    [Description("@#width")]
    public VueStringNumberValue? Width { get; init; }

    /// <summary>
    /// 此列允许的最小宽度。
    /// </summary>
    [Description("@#minWidth")]
    public VueStringNumberValue? MinWidth { get; init; }

    /// <summary>
    /// 此列允许的最大宽度。
    /// </summary>
    [Description("@#maxWidth")]
    public VueStringNumberValue? MaxWidth { get; init; }

    /// <summary>
    /// 禁止此列单元格内容自动换行。
    /// </summary>
    [Description("@#nowrap")]
    public bool? Nowrap { get; init; }

    /// <summary>
    /// 是否将此列固定在水平滚动区域的边缘。
    /// </summary>
    [Description("@#fixed")]
    public bool? Fixed { get; init; }

    /// <summary>
    /// 当前条目的直接子项，形成嵌套分组或树形结构。
    /// </summary>
    [Description("@#children")]
    public VuetifyDataTableHeader[]? Children { get; init; }
}

/// <summary>
/// 用于 VuetifyDataTableHeader.Align 的可选取值。
/// 此列单元格内容的对齐方式。
/// </summary>
[String]
public enum VuetifyDataTableHeaderAlign
{
    /// <summary>
    /// 逻辑起始端，方向随 RTL 布局变化；上游取值为 “start”。
    /// </summary>
    [Description("@#start")]
    Start,

    /// <summary>
    /// 逻辑结束端，方向随 RTL 布局变化；上游取值为 “end”。
    /// </summary>
    [Description("@#end")]
    End,

    /// <summary>
    /// 居中；上游取值为 “center”。
    /// </summary>
    [Description("@#center")]
    Center
}

/// <summary>
/// 用于 VuetifyDataTableSortItem.Order 的可选取值。
/// 当前排序字段的升序或降序方向。
/// </summary>
[String]
public enum VuetifyDataTableSortOrder
{
    /// <summary>
    /// 按升序排序；上游取值为 “asc”。
    /// </summary>
    [Description("@#asc")]
    Asc,

    /// <summary>
    /// 按降序排序；上游取值为 “desc”。
    /// </summary>
    [Description("@#desc")]
    Desc
}

/// <summary>
/// 用于 VDataIterator.SelectStrategy、VDataTable.SelectStrategy 的可选取值。
/// Defines the strategy of selecting items in the list. Possible values are: 'single' (only one item can be selected at a time), 'page' ('Select all' button will select only items on the current page), 'all' ('Select all' button will select all items in the list).
/// </summary>
[String]
public enum VuetifyDataTableSelectStrategy
{
    /// <summary>
    /// 只允许单选；上游取值为 “single”。
    /// </summary>
    [Description("@#single")]
    Single,

    /// <summary>
    /// 作用于当前页条目；上游取值为 “page”。
    /// </summary>
    [Description("@#page")]
    Page,

    /// <summary>
    /// 作用于所有条目；上游取值为 “all”。
    /// </summary>
    [Description("@#all")]
    All
}

/// <summary>
/// 用于 VDataTable.Items、VDataTable.CurrentItemsChanged 的参数类型。
/// An array of strings or objects used for automatically generating children components.
/// 当前可见行变化时的回调。
/// Callback when currently visible items change.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableItemsCollectionBuilder), nameof(VuetifyDataTableItemsCollectionBuilder.Create))]
public readonly union VuetifyDataTableItems(VuetifyDataTableItem[]) : IEnumerable<VuetifyDataTableItem>
{
    /// <summary>
    /// 读取当前值的 VuetifyDataTableItem[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDataTableItem[]? AsArray
        => Value is VuetifyDataTableItem[] value ? value : default(VuetifyDataTableItem[]?);

    /// <summary>
    /// 将 VuetifyDataTableItem[] 值转换为 VuetifyDataTableItems，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItems(VuetifyDataTableItem[] items)
        => new(items);

    IEnumerator<VuetifyDataTableItem> IEnumerable<VuetifyDataTableItem>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItem>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItem>)this).GetEnumerator();
}


/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableItemsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    public static VuetifyDataTableItems Create(ReadOnlySpan<VuetifyDataTableItem> items)
        => items.ToArray();
}

/// <summary>
/// 用于 VDataTableSlotContext.Items、VDataTableSlotContext.InternalItems、VDataTableSlotContext.GroupedItems、VDataTableItemSlotContext.Item、VDataTableItemSlotContext.InternalItem、VDataTableGroupHeaderSlotContext.Item、VuetifyDataTableRowPropsContext.Item、VuetifyDataTableRowPropsContext.InternalItem、VuetifyDataTableCellPropsContext.Item、VuetifyDataTableCellPropsContext.InternalItem 的参数类型。
/// 当前组件处理后的条目集合，顺序与当前渲染顺序一致。
/// 当前内部条目集合，包含转换后的渲染和选择信息。
/// 按 groupBy 组织后的分组条目。
/// 当前渲染或操作的数据项。
/// Vuetify 转换后的内部数据项；包含原始数据及用于渲染、选择的附加信息。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableItem : IEnumerable
{
    /// <summary>
    /// 按字段名读取或写入表格条目的业务值。
    /// </summary>
    public extern VueValue? this[string key] { get; set; }

    /// <summary>
    /// 按字段名写入当前对象的业务值，支持 C# 集合初始化器；同名字段使用新值覆盖。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <param name="key">字段名称；按该名称写入当前数据项的属性。</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern void Add(string key, VueValue value);

    extern IEnumerator IEnumerable.GetEnumerator();
}

/// <summary>
/// 用于 VDataTable.ModelValue、VDataTable.ModelValueChanged、VDataTable.Expanded、VDataTable.ExpandedChanged 的参数类型。
/// The v-model value of the component. If component supports the **multiple** prop, this defaults to an empty array.
/// 选中行变化时的回调。
/// Callback when selected rows change.
/// Array of expanded items. Can be bound to external variable using **v-model:expanded**.
/// 展开行变化时的回调。
/// Callback when expanded rows change.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableSelectedValuesCollectionBuilder), nameof(VuetifyDataTableSelectedValuesCollectionBuilder.Create))]
public readonly union VuetifyDataTableSelectedValues(VueValue[]) : IEnumerable<VueValue>
{
    /// <summary>
    /// 读取当前值的 VueValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VueValue[]? AsArray
        => Value is VueValue[] value ? value : default(VueValue[]?);

    /// <summary>
    /// 将 VueValue[] 值转换为 VuetifyDataTableSelectedValues，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableSelectedValues(VueValue[] values)
        => new(values);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableSelectedValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableSelectedValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableSelectedValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableSelectedValues(double[] values)
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
public static class VuetifyDataTableSelectedValuesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifyDataTableSelectedValues Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

/// <summary>
/// 用于 VDataIterator.SortBy、VDataIterator.SortByChanged、VDataIterator.GroupBy、VDataIterator.GroupByChanged、VDataTable.SortBy、VDataTable.SortByChanged、VDataTable.GroupBy、VDataTable.GroupByChanged 的参数类型。
/// Array of column keys and sort orders that determines the sort order of the table.
/// 排序规则变更回调。
/// Callback invoked when the sort criteria change.
/// Configures attributes (and sort order) to group items together.
/// 分组规则变更回调。
/// Callback invoked when the group-by criteria change.
/// Changes which item property (or properties) should be used for sort order. Can be bound to external variable using **v-model:sortBy**..
/// 排序规则变化时的回调。
/// Callback when sort criteria change.
/// Configures attributes (and sort order) to group items together. Can be customized further with `group-header` and `group-summary` slots.
/// 分组规则变化时的回调。
/// Callback when group-by criteria change.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableSortItemsCollectionBuilder), nameof(VuetifyDataTableSortItemsCollectionBuilder.Create))]
public readonly union VuetifyDataTableSortItems(VuetifyDataTableSortItem[]) : IEnumerable<VuetifyDataTableSortItem>
{
    /// <summary>
    /// 读取当前值的 VuetifyDataTableSortItem[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDataTableSortItem[]? AsArray
        => Value is VuetifyDataTableSortItem[] value ? value : default(VuetifyDataTableSortItem[]?);

    /// <summary>
    /// 将 VuetifyDataTableSortItem[] 值转换为 VuetifyDataTableSortItems，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableSortItems(VuetifyDataTableSortItem[] items)
        => new(items);

    IEnumerator<VuetifyDataTableSortItem> IEnumerable<VuetifyDataTableSortItem>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableSortItem>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableSortItem>)this).GetEnumerator();
}


/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableSortItemsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    public static VuetifyDataTableSortItems Create(ReadOnlySpan<VuetifyDataTableSortItem> items)
        => items.ToArray();
}

/// <summary>
/// 用于 VDataIteratorSlotContext.SortBy、VDataTableSlotContext.SortBy、VDataTableHeadersSlotContext.SortBy、VDataTableHeaderCellSlotContext.SortBy、VuetifyDataTableOptions.SortBy、VuetifyDataTableOptions.GroupBy 的参数类型。
/// 当前排序字段和各字段的排序方向，数组顺序表示排序优先级。
/// 用于分组的字段和组排序方向。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableSortItem
{
    /// <summary>
    /// 列或排序字段的标识，应与条目中的对应数据字段匹配。
    /// </summary>
    [Description("@#key")]
    public string? Key { get; init; }

    /// <summary>
    /// 当前排序字段的升序或降序方向。
    /// </summary>
    [Description("@#order")]
    public VuetifyDataTableSortOrder? Order { get; init; }
}

/// <summary>
/// 用于 VDataIterator.OptionsChanged、VDataTable.OptionsChanged 的参数类型。
/// 分页/排序/分组选项变更回调。
/// Callback invoked when pagination/sort/group options change.
/// 分页、排序等选项变化时的回调。
/// Callback when pagination or sort options change.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableOptions
{
    /// <summary>
    /// 当前页码，从 1 开始。
    /// </summary>
    [Description("@#page")]
    public int? Page { get; init; }

    /// <summary>
    /// 每页显示的条目数；-1 表示显示全部条目。
    /// </summary>
    [Description("@#itemsPerPage")]
    public int? ItemsPerPage { get; init; }

    /// <summary>
    /// 当前排序字段和各字段的排序方向，数组顺序表示排序优先级。
    /// </summary>
    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    /// <summary>
    /// 用于分组的字段和组排序方向。
    /// </summary>
    [Description("@#groupBy")]
    public VuetifyDataTableSortItem[]? GroupBy { get; init; }

    /// <summary>
    /// 用于过滤表格条目的搜索文本。
    /// </summary>
    [Description("@#search")]
    public string? Search { get; init; }
}

/// <summary>
/// 用于 VDataTable.ItemsPerPageOptions 的参数类型。
/// Array of options to show in the items-per-page dropdown.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableItemsPerPageOptionsCollectionBuilder), nameof(VuetifyDataTableItemsPerPageOptionsCollectionBuilder.Create))]
public readonly union VuetifyDataTableItemsPerPageOptions(VuetifyDataTableItemsPerPageOption[]) : IEnumerable<VuetifyDataTableItemsPerPageOption>
{
    /// <summary>
    /// 读取当前值的 VuetifyDataTableItemsPerPageOption[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDataTableItemsPerPageOption[]? AsArray
        => Value is VuetifyDataTableItemsPerPageOption[] value ? value : default(VuetifyDataTableItemsPerPageOption[]?);

    /// <summary>
    /// 将 VuetifyDataTableItemsPerPageOption[] 值转换为 VuetifyDataTableItemsPerPageOptions，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOptions(VuetifyDataTableItemsPerPageOption[] options)
        => new(options);

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOptions(Number[] options)
        => new(Array.ConvertAll(options, static value => (VuetifyDataTableItemsPerPageOption)value));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOptions(int[] options)
        => new(Array.ConvertAll(options, static value => (VuetifyDataTableItemsPerPageOption)value));

    IEnumerator<VuetifyDataTableItemsPerPageOption> IEnumerable<VuetifyDataTableItemsPerPageOption>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItemsPerPageOption>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItemsPerPageOption>)this).GetEnumerator();
}


/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableItemsPerPageOptionsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    public static VuetifyDataTableItemsPerPageOptions Create(ReadOnlySpan<VuetifyDataTableItemsPerPageOption> options)
        => options.ToArray();
}

/// <summary>
/// C# 联合参数，允许 Number, VuetifyDataTableItemsPerPageOptionItem。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDataTableItemsPerPageOption(Number, VuetifyDataTableItemsPerPageOptionItem)
{
    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 VuetifyDataTableItemsPerPageOptionItem 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDataTableItemsPerPageOptionItem? AsItem
        => Value is VuetifyDataTableItemsPerPageOptionItem value ? value : default(VuetifyDataTableItemsPerPageOptionItem?);

    /// <summary>
    /// 将 Number 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(Number value)
        => new(value);

    /// <summary>
    /// 将 VuetifyDataTableItemsPerPageOptionItem 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(VuetifyDataTableItemsPerPageOptionItem value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyDataTableItemsPerPageOption，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableItemsPerPageOption(decimal value)
        => new((Number)value);
}


/// <summary>
/// 分页大小选项，分别指定显示标签与每页条目数。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableItemsPerPageOptionItem
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public string? Title { get; init; }

    /// <summary>
    /// 当前控件或条目显示和处理的数值。
    /// </summary>
    [Description("@#value")]
    public int? Value { get; init; }
}

/// <summary>
/// 用于 VDataTable.RowProps 的参数类型。
/// An object of additional props to be passed to each `&lt;tr&gt;` in the table body. Also accepts a function that will be called for each row.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDataTableRowProps(VueProps, VuetifyDataTableRowPropsCallback)
{
    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsProps
        => Value is VueProps value ? value : default(VueProps?);

    /// <summary>
    /// 读取当前值的 VuetifyDataTableRowPropsCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDataTableRowPropsCallback? AsCallback
        => Value is VuetifyDataTableRowPropsCallback value ? value : default(VuetifyDataTableRowPropsCallback?);

    /// <summary>
    /// 将 VueProps 值转换为 VuetifyDataTableRowProps，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableRowProps(VueProps value)
        => new(value);

    /// <summary>
    /// 将 VueDictionary 值转换为 VuetifyDataTableRowProps，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableRowProps(VueDictionary value)
        => new(value);

    /// <summary>
    /// 将 VuetifyDataTableRowPropsCallback 值转换为 VuetifyDataTableRowProps，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableRowProps(VuetifyDataTableRowPropsCallback value)
        => new(value);
}


/// <summary>
/// 用于 VuetifyDataTableRowProps.AsCallback 的回调签名。
/// 读取当前值的 VuetifyDataTableRowPropsCallback 分支；不属于该分支时返回 null。
/// </summary>
public delegate VueProps? VuetifyDataTableRowPropsCallback(VuetifyDataTableRowPropsContext context);

/// <summary>
/// 用于 VDataTable.CellProps 的参数类型。
/// An object of additional props to be passed to each `&lt;td&gt;` in the table body. Also accepts a function that will be called for each cell. If the same prop is defined both here and in `cellProps` in a headers object, the value from the headers object will be used.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDataTableCellProps(VueProps, VuetifyDataTableCellPropsCallback)
{
    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsProps
        => Value is VueProps value ? value : default(VueProps?);

    /// <summary>
    /// 读取当前值的 VuetifyDataTableCellPropsCallback 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDataTableCellPropsCallback? AsCallback
        => Value is VuetifyDataTableCellPropsCallback value ? value : default(VuetifyDataTableCellPropsCallback?);

    /// <summary>
    /// 将 VueProps 值转换为 VuetifyDataTableCellProps，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableCellProps(VueProps value)
        => new(value);

    /// <summary>
    /// 将 VueDictionary 值转换为 VuetifyDataTableCellProps，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableCellProps(VueDictionary value)
        => new(value);

    /// <summary>
    /// 将 VuetifyDataTableCellPropsCallback 值转换为 VuetifyDataTableCellProps，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataTableCellProps(VuetifyDataTableCellPropsCallback value)
        => new(value);
}


/// <summary>
/// 用于 VuetifyDataTableCellProps.AsCallback 的回调签名。
/// 读取当前值的 VuetifyDataTableCellPropsCallback 分支；不属于该分支时返回 null。
/// </summary>
public delegate VueProps? VuetifyDataTableCellPropsCallback(VuetifyDataTableCellPropsContext context);
