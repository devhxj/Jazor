using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 树形视图节点激活策略。
/// Vuetify treeview node activation strategy.
/// </summary>
[String]
public enum VuetifyTreeviewActiveStrategy
{
    /// <summary>
    /// 只允许一个叶子节点选择；上游取值为 “single-leaf”。
    /// </summary>
    [Description("@#single-leaf")]
    SingleLeaf,

    /// <summary>
    /// 只允许叶子节点选择；上游取值为 “leaf”。
    /// </summary>
    [Description("@#leaf")]
    Leaf,

    /// <summary>
    /// 各节点独立选择，不传播父子状态；上游取值为 “independent”。
    /// </summary>
    [Description("@#independent")]
    Independent,

    /// <summary>
    /// 只允许一个节点独立选择；上游取值为 “single-independent”。
    /// </summary>
    [Description("@#single-independent")]
    SingleIndependent
}

/// <summary>
/// Vuetify 树形视图节点选择策略。
/// Vuetify treeview node selection strategy.
/// </summary>
[String]
public enum VuetifyTreeviewSelectStrategy
{
    /// <summary>
    /// 只允许一个叶子节点选择；上游取值为 “single-leaf”。
    /// </summary>
    [Description("@#single-leaf")]
    SingleLeaf,

    /// <summary>
    /// 只允许叶子节点选择；上游取值为 “leaf”。
    /// </summary>
    [Description("@#leaf")]
    Leaf,

    /// <summary>
    /// 各节点独立选择，不传播父子状态；上游取值为 “independent”。
    /// </summary>
    [Description("@#independent")]
    Independent,

    /// <summary>
    /// 只允许一个节点独立选择；上游取值为 “single-independent”。
    /// </summary>
    [Description("@#single-independent")]
    SingleIndependent,

    /// <summary>
    /// 父节点选择传播到子节点，并根据子节点状态计算父节点的中间状态；上游取值为 “classic”。
    /// </summary>
    [Description("@#classic")]
    Classic,

    /// <summary>
    /// 选中分支时以可覆盖该分支的父节点表示选择结果；上游取值为 “trunk”。
    /// </summary>
    [Description("@#trunk")]
    Trunk
}

/// <summary>
/// Vuetify 树形视图节点选择状态。
/// Vuetify treeview node selection state.
/// </summary>
[String]
public enum VuetifyTreeviewSelectionState
{
    /// <summary>
    /// 选中状态；上游取值为 “on”。
    /// </summary>
    [Description("@#on")]
    On,

    /// <summary>
    /// 未选中状态；上游取值为 “off”。
    /// </summary>
    [Description("@#off")]
    Off,

    /// <summary>
    /// 部分子项选中的中间状态；上游取值为 “indeterminate”。
    /// </summary>
    [Description("@#indeterminate")]
    Indeterminate
}

/// <summary>
/// Vuetify 树形视图选中值的擦除值联合类型。
/// Erased value union for Vuetify treeview selected values.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyTreeviewValuesCollectionBuilder), nameof(VuetifyTreeviewValuesCollectionBuilder.Create))]
public readonly union VuetifyTreeviewValues(VueValue[]) : IEnumerable<VueValue>
{
    /// <summary>
    /// 读取当前值的 VueValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VueValue[]? AsArray => Value as VueValue[];

    /// <summary>
    /// 将 VueValue[] 值转换为 VuetifyTreeviewValues，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewValues(VueValue[] values)
        => new(values);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 bool[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewValues(bool[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 VueProps[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewValues(VueProps[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 VueDictionary[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewValues(VueDictionary[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewValues(double[] values)
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
public static class VuetifyTreeviewValuesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifyTreeviewValues Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

/// <summary>
/// Vuetify 树形视图项目列表的擦除值联合类型。
/// Erased value union for Vuetify treeview item collections.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyTreeviewItemsCollectionBuilder), nameof(VuetifyTreeviewItemsCollectionBuilder.Create))]
public readonly union VuetifyTreeviewItems(VuetifyTreeviewItemValue[]) : IEnumerable<VuetifyTreeviewItemValue>
{
    /// <summary>
    /// 读取当前值的 VuetifyTreeviewItemValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTreeviewItemValue[]? AsArray => Value as VuetifyTreeviewItemValue[];

    /// <summary>
    /// 将 VuetifyTreeviewItemValue[] 值转换为 VuetifyTreeviewItems，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItems(VuetifyTreeviewItemValue[] items)
        => new(items);

    /// <summary>
    /// 将 VuetifyTreeviewItem[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItems(VuetifyTreeviewItem[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyTreeviewItemValue)item));

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyTreeviewItemValue)item));

    IEnumerator<VuetifyTreeviewItemValue> IEnumerable<VuetifyTreeviewItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifyTreeviewItemValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyTreeviewItemValue>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyTreeviewItemsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    public static VuetifyTreeviewItems Create(ReadOnlySpan<VuetifyTreeviewItemValue> items)
        => items.ToArray();
}

/// <summary>
/// Vuetify 树形视图单个项目的擦除值联合类型。
/// Erased value union for a single Vuetify treeview item.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyTreeviewItemValue(
    string,
    VuetifyTreeviewItem,
    Number,
    bool,
    VueProps)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsString => Value as string;

    /// <summary>
    /// 读取当前值的 VuetifyTreeviewItem 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTreeviewItem? AsItem => Value as VuetifyTreeviewItem;

    /// <summary>
    /// 读取当前值的 Number 分支；不属于该分支时返回 null。
    /// </summary>
    public Number? AsNumber => Value is Number value ? value : default(Number?);

    /// <summary>
    /// 读取当前值的 bool 分支；不属于该分支时返回 null。
    /// </summary>
    public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>
    /// 读取当前值的 VueProps 分支；不属于该分支时返回 null。
    /// </summary>
    public VueProps? AsObject => Value as VueProps;

    /// <summary>
    /// 将 string 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(string value)
        => new(value);

    /// <summary>
    /// 将 VuetifyTreeviewItem 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(VuetifyTreeviewItem value)
        => new(value);

    /// <summary>
    /// 将 Number 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(Number value)
        => new(value);

    /// <summary>
    /// 将 bool 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(bool value)
        => new(value);

    /// <summary>
    /// 将 VueProps 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(VueProps value)
        => new(value);

    /// <summary>
    /// 将 VueDictionary 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(VueDictionary value)
        => new(value);

    /// <summary>
    /// 将 byte 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(byte value)
        => new((Number)value);

    /// <summary>
    /// 将 sbyte 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(sbyte value)
        => new((Number)value);

    /// <summary>
    /// 将 short 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(short value)
        => new((Number)value);

    /// <summary>
    /// 将 ushort 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(ushort value)
        => new((Number)value);

    /// <summary>
    /// 将 int 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(int value)
        => new((Number)value);

    /// <summary>
    /// 将 uint 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(uint value)
        => new((Number)value);

    /// <summary>
    /// 将 float 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(float value)
        => new((Number)value);

    /// <summary>
    /// 将 double 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(double value)
        => new((Number)value);

    /// <summary>
    /// 将 decimal 值转换为 VuetifyTreeviewItemValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewItemValue(decimal value)
        => new((Number)value);
}

/// <summary>
/// Vuetify 树形视图项目定义。
/// Vuetify treeview item definition.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifyTreeviewItem
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public VuetifyTextValue? Title { get; init; }

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
    public VuetifyTreeviewItems? Children { get; init; }

    /// <summary>
    /// 调用方提供的原始数据，供自定义渲染读取业务字段。
    /// </summary>
    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

/// <summary>
/// 用于 VTreeview.LoadChildren 的回调签名。
/// A function used when dynamically loading children. If this prop is set, then the supplied function will be run if expanding an item that has a `item-children` property that is an empty array. Supports returning a Promise.
/// </summary>
public delegate IPromise VuetifyTreeviewLoadChildrenCallback(VueValue? item);

/// <summary>
/// 用于 VuetifyTreeviewActiveStrategyValue.AsFactory 的回调签名。
/// 读取当前值的 VuetifyTreeviewActiveStrategyFactory 分支；不属于该分支时返回 null。
/// </summary>
public delegate VuetifyTreeviewActiveStrategyDefinition VuetifyTreeviewActiveStrategyFactory(bool mandatory);

/// <summary>
/// 用于 VuetifyTreeviewSelectStrategyValue.AsFactory 的回调签名。
/// 读取当前值的 VuetifyTreeviewSelectStrategyFactory 分支；不属于该分支时返回 null。
/// </summary>
public delegate VuetifyTreeviewSelectStrategyDefinition VuetifyTreeviewSelectStrategyFactory(bool mandatory);

/// <summary>
/// Vuetify 树形视图激活策略值的擦除值联合类型。
/// Erased value union for Vuetify treeview active strategy.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyTreeviewActiveStrategyValue(
    VuetifyTreeviewActiveStrategy,
    VuetifyTreeviewActiveStrategyDefinition,
    VuetifyTreeviewActiveStrategyFactory)
{
    /// <summary>
    /// 读取当前值的 VuetifyTreeviewActiveStrategy 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTreeviewActiveStrategy? AsName
        => Value is VuetifyTreeviewActiveStrategy value ? value : default(VuetifyTreeviewActiveStrategy?);

    /// <summary>
    /// 读取当前值的 VuetifyTreeviewActiveStrategyDefinition 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTreeviewActiveStrategyDefinition? AsDefinition
        => Value as VuetifyTreeviewActiveStrategyDefinition;

    /// <summary>
    /// 读取当前值的 VuetifyTreeviewActiveStrategyFactory 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTreeviewActiveStrategyFactory? AsFactory
        => Value as VuetifyTreeviewActiveStrategyFactory;

    /// <summary>
    /// 将 VuetifyTreeviewActiveStrategy 值转换为 VuetifyTreeviewActiveStrategyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategy value)
        => new(value);

    /// <summary>
    /// 将 VuetifyTreeviewActiveStrategyDefinition 值转换为 VuetifyTreeviewActiveStrategyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategyDefinition value)
        => new(value);

    /// <summary>
    /// 将 VuetifyTreeviewActiveStrategyFactory 值转换为 VuetifyTreeviewActiveStrategyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewActiveStrategyValue(VuetifyTreeviewActiveStrategyFactory value)
        => new(value);
}

/// <summary>
/// Vuetify 树形视图选择策略值的擦除值联合类型。
/// Erased value union for Vuetify treeview select strategy.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyTreeviewSelectStrategyValue(
    VuetifyTreeviewSelectStrategy,
    VuetifyTreeviewSelectStrategyDefinition,
    VuetifyTreeviewSelectStrategyFactory)
{
    /// <summary>
    /// 读取当前值的 VuetifyTreeviewSelectStrategy 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTreeviewSelectStrategy? AsName
        => Value is VuetifyTreeviewSelectStrategy value ? value : default(VuetifyTreeviewSelectStrategy?);

    /// <summary>
    /// 读取当前值的 VuetifyTreeviewSelectStrategyDefinition 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTreeviewSelectStrategyDefinition? AsDefinition
        => Value as VuetifyTreeviewSelectStrategyDefinition;

    /// <summary>
    /// 读取当前值的 VuetifyTreeviewSelectStrategyFactory 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyTreeviewSelectStrategyFactory? AsFactory
        => Value as VuetifyTreeviewSelectStrategyFactory;

    /// <summary>
    /// 将 VuetifyTreeviewSelectStrategy 值转换为 VuetifyTreeviewSelectStrategyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewSelectStrategyValue(VuetifyTreeviewSelectStrategy value)
        => new(value);

    /// <summary>
    /// 将 VuetifyTreeviewSelectStrategyDefinition 值转换为 VuetifyTreeviewSelectStrategyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewSelectStrategyValue(VuetifyTreeviewSelectStrategyDefinition value)
        => new(value);

    /// <summary>
    /// 将 VuetifyTreeviewSelectStrategyFactory 值转换为 VuetifyTreeviewSelectStrategyValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyTreeviewSelectStrategyValue(VuetifyTreeviewSelectStrategyFactory value)
        => new(value);
}

/// <summary>
/// 自定义树节点激活策略的模型转换与激活操作。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewActiveStrategyDefinition : VueProps
{
    /// <summary>
    /// 按当前树形激活策略更新激活节点集合。
    /// </summary>
    [Description("@#activate")]
    public VuetifyTreeviewActiveStrategyActivateCallback? Activate { get; init; }

    /// <summary>
    /// 将外部模型值转换为策略内部的状态集合。
    /// </summary>
    [Description("@#in")]
    public VuetifyTreeviewActiveStrategyTransformInCallback? In { get; init; }

    /// <summary>
    /// 将策略内部的状态集合转换为对外更新的模型值。
    /// </summary>
    [Description("@#out")]
    public VuetifyTreeviewActiveStrategyTransformOutCallback? Out { get; init; }
}

/// <summary>
/// 自定义树节点选择策略的输入、输出转换和状态更新操作。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewSelectStrategyDefinition : VueProps
{
    /// <summary>
    /// 根据传入的目标和布尔值更新选择状态。
    /// </summary>
    [Description("@#select")]
    public VuetifyTreeviewSelectStrategySelectCallback? Select { get; init; }

    /// <summary>
    /// 将外部模型值转换为策略内部的状态集合。
    /// </summary>
    [Description("@#in")]
    public VuetifyTreeviewSelectStrategyTransformInCallback? In { get; init; }

    /// <summary>
    /// 将策略内部的状态集合转换为对外更新的模型值。
    /// </summary>
    [Description("@#out")]
    public VuetifyTreeviewSelectStrategyTransformOutCallback? Out { get; init; }
}

/// <summary>
/// 用于 VuetifyTreeviewActiveStrategyDefinition.Activate 的回调签名。
/// 按当前树形激活策略更新激活节点集合。
/// </summary>
public delegate Set<VueValue> VuetifyTreeviewActiveStrategyActivateCallback(VuetifyTreeviewActiveStrategyActivateContext context);

/// <summary>
/// 用于 VuetifyTreeviewActiveStrategyDefinition.In 的回调签名。
/// 将外部模型值转换为策略内部的状态集合。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="children">节点标识到其直接子节点标识数组的映射。</param>
/// <param name="parents">节点标识到其父节点标识的映射。</param>
public delegate Set<VueValue> VuetifyTreeviewActiveStrategyTransformInCallback(
    VuetifyTreeviewValues? value,
    Map<VueValue, VueValue[]> children,
    Map<VueValue, VueValue> parents);

/// <summary>
/// 用于 VuetifyTreeviewActiveStrategyDefinition.Out 的回调签名。
/// 将策略内部的状态集合转换为对外更新的模型值。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="children">节点标识到其直接子节点标识数组的映射。</param>
/// <param name="parents">节点标识到其父节点标识的映射。</param>
public delegate VuetifyTreeviewValues VuetifyTreeviewActiveStrategyTransformOutCallback(
    Set<VueValue> value,
    Map<VueValue, VueValue[]> children,
    Map<VueValue, VueValue> parents);

/// <summary>
/// 用于 VuetifyTreeviewSelectStrategyDefinition.Select 的回调签名。
/// 根据传入的目标和布尔值更新选择状态。
/// </summary>
public delegate Map<VueValue, VuetifyTreeviewSelectionState> VuetifyTreeviewSelectStrategySelectCallback(
    VuetifyTreeviewSelectStrategySelectContext context);

/// <summary>
/// 用于 VuetifyTreeviewSelectStrategyDefinition.In 的回调签名。
/// 将外部模型值转换为策略内部的状态集合。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="children">节点标识到其直接子节点标识数组的映射。</param>
/// <param name="parents">节点标识到其父节点标识的映射。</param>
public delegate Map<VueValue, VuetifyTreeviewSelectionState> VuetifyTreeviewSelectStrategyTransformInCallback(
    VuetifyTreeviewValues? value,
    Map<VueValue, VueValue[]> children,
    Map<VueValue, VueValue> parents);

/// <summary>
/// 用于 VuetifyTreeviewSelectStrategyDefinition.Out 的回调签名。
/// 将策略内部的状态集合转换为对外更新的模型值。
/// </summary>
/// <param name="value">要传入的值，保持其声明的类型和数据。</param>
/// <param name="children">节点标识到其直接子节点标识数组的映射。</param>
/// <param name="parents">节点标识到其父节点标识的映射。</param>
public delegate VuetifyTreeviewValues VuetifyTreeviewSelectStrategyTransformOutCallback(
    Map<VueValue, VuetifyTreeviewSelectionState> value,
    Map<VueValue, VueValue[]> children,
    Map<VueValue, VueValue> parents);

/// <summary>
/// 树形激活策略接收的目标节点、新状态及父子关系映射。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewActiveStrategyActivateContext : VueProps
{
    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。
    /// </summary>
    [Description("@#id")]
    public VueValue? Id { get; init; }

    /// <summary>
    /// 本次选择或激活操作要求的新状态。
    /// </summary>
    [Description("@#value")]
    public bool Value { get; init; }

    /// <summary>
    /// 当前激活节点 id 的集合。
    /// </summary>
    [Description("@#activated")]
    public Set<VueValue>? Activated { get; init; }

    /// <summary>
    /// 父节点 id 到直接子节点 id 数组的映射。
    /// </summary>
    [Description("@#children")]
    public Map<VueValue, VueValue[]>? Children { get; init; }

    /// <summary>
    /// 子节点 id 到父节点 id 的映射，供树形策略向上遍历。
    /// </summary>
    [Description("@#parents")]
    public Map<VueValue, VueValue>? Parents { get; init; }

    /// <summary>
    /// 触发本次树形选择或激活操作的原生事件。
    /// </summary>
    [Description("@#event")]
    public EventRef? Event { get; init; }
}

/// <summary>
/// 树形选择策略接收的节点、新状态、当前选择映射及父子关系。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewSelectStrategySelectContext : VueProps
{
    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。
    /// </summary>
    [Description("@#id")]
    public VueValue? Id { get; init; }

    /// <summary>
    /// 本次选择或激活操作要求的新状态。
    /// </summary>
    [Description("@#value")]
    public bool Value { get; init; }

    /// <summary>
    /// 各节点 id 对应的选中、未选中或中间状态。
    /// </summary>
    [Description("@#selected")]
    public Map<VueValue, VuetifyTreeviewSelectionState>? Selected { get; init; }

    /// <summary>
    /// 父节点 id 到直接子节点 id 数组的映射。
    /// </summary>
    [Description("@#children")]
    public Map<VueValue, VueValue[]>? Children { get; init; }

    /// <summary>
    /// 子节点 id 到父节点 id 的映射，供树形策略向上遍历。
    /// </summary>
    [Description("@#parents")]
    public Map<VueValue, VueValue>? Parents { get; init; }

    /// <summary>
    /// 触发本次树形选择或激活操作的原生事件。
    /// </summary>
    [Description("@#event")]
    public EventRef? Event { get; init; }
}

/// <summary>
/// 用于 VTreeview.OnOpenClick、VTreeview.OnSelectClick 的参数类型。
/// 展开点击事件。
/// Open click event.
/// 选中点击事件。
/// Select click event.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewClickPayload : VueProps
{
    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。
    /// </summary>
    [Description("@#id")]
    public VueValue? Id { get; init; }

    /// <summary>
    /// 本次选择或激活操作要求的新状态。
    /// </summary>
    [Description("@#value")]
    public bool Value { get; init; }

    /// <summary>
    /// 从根节点到当前节点的 id 路径。
    /// </summary>
    [Description("@#path")]
    public VueValue[]? Path { get; init; }
}

/// <summary>
/// 用于 VuetifyTreeviewInternalItem.Children、VTreeviewNodeSlotContext.InternalItem、VTreeviewTitleSlotContext.InternalItem、VTreeviewSubtitleSlotContext.InternalItem、VTreeviewItemSlotContext.InternalItem 的参数类型。
/// 当前条目的直接子项，形成嵌套分组或树形结构。
/// Vuetify 转换后的内部数据项；包含原始数据及用于渲染、选择的附加信息。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTreeviewInternalItem : VueProps
{
    /// <summary>
    /// 内部条目的类别标记，用于区分普通条目、分组等数据形状。
    /// </summary>
    [Description("@#type")]
    public string? Type { get; init; }

    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public VuetifyTextValue? Title { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public VueValue? Value { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VuetifyItemProps? Props { get; init; }

    /// <summary>
    /// 当前条目的直接子项，形成嵌套分组或树形结构。
    /// </summary>
    [Description("@#children")]
    public VuetifyTreeviewInternalItem[]? Children { get; init; }

    /// <summary>
    /// 调用方提供的原始数据，供自定义渲染读取业务字段。
    /// </summary>
    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

/// <summary>
/// 用于 VTreeview.Prepend、VTreeview.Append 的参数类型。
/// 前置插槽。
/// Prepend slot.
/// 后置插槽。
/// Append slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VTreeviewNodeSlotContext
{
    /// <summary>
    /// 当前目标是否处于激活状态；在浮层上下文中表示是否显示。
    /// </summary>
    [Description("@#isActive")]
    public bool IsActive { get; init; }

    /// <summary>
    /// 当前节点或分组是否已展开。
    /// </summary>
    [Description("@#isOpen")]
    public bool IsOpen { get; init; }

    /// <summary>
    /// 当前条目是否已选中。
    /// </summary>
    [Description("@#isSelected")]
    public bool IsSelected { get; init; }

    /// <summary>
    /// 当前节点是否处于部分子项选中的中间状态。
    /// </summary>
    [Description("@#isIndeterminate")]
    public bool IsIndeterminate { get; init; }

    /// <summary>
    /// 根据传入的目标和布尔值更新选择状态。
    /// </summary>
    [Description("@#select")]
    public VListItemSelectCallback? Select { get; init; }

    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VueValue? Item { get; init; }

    /// <summary>
    /// Vuetify 转换后的内部数据项；包含原始数据及用于渲染、选择的附加信息。
    /// </summary>
    [Description("@#internalItem")]
    public VuetifyTreeviewInternalItem? InternalItem { get; init; }
}

/// <summary>
/// 用于 VTreeview.TitleContent 的参数类型。
/// 标题内容插槽。
/// Title content slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VTreeviewTitleSlotContext
{
    /// <summary>
    /// 条目的标题内容，用于默认显示文本。
    /// </summary>
    [Description("@#title")]
    public VuetifyTextValue? Title { get; init; }

    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VueValue? Item { get; init; }

    /// <summary>
    /// Vuetify 转换后的内部数据项；包含原始数据及用于渲染、选择的附加信息。
    /// </summary>
    [Description("@#internalItem")]
    public VuetifyTreeviewInternalItem? InternalItem { get; init; }
}

/// <summary>
/// 用于 VTreeview.SubtitleContent 的参数类型。
/// 副标题内容插槽。
/// Subtitle content slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VTreeviewSubtitleSlotContext
{
    /// <summary>
    /// 条目标题下方的补充文本。
    /// </summary>
    [Description("@#subtitle")]
    public VuetifyTextValue? Subtitle { get; init; }

    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VueValue? Item { get; init; }

    /// <summary>
    /// Vuetify 转换后的内部数据项；包含原始数据及用于渲染、选择的附加信息。
    /// </summary>
    [Description("@#internalItem")]
    public VuetifyTreeviewInternalItem? InternalItem { get; init; }
}

/// <summary>
/// 用于 VTreeview.ItemContent 的参数类型。
/// 项内容插槽。
/// Item content slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VTreeviewItemSlotContext : VueProps
{
    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VuetifyItemProps? Props { get; init; }

    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VueValue? Item { get; init; }

    /// <summary>
    /// Vuetify 转换后的内部数据项；包含原始数据及用于渲染、选择的附加信息。
    /// </summary>
    [Description("@#internalItem")]
    public VuetifyTreeviewInternalItem? InternalItem { get; init; }
}

/// <summary>
/// 用于 VTreeview.Header、VTreeview.Divider、VTreeview.Subheader 的参数类型。
/// 头部插槽。
/// Header slot.
/// 分隔线插槽。
/// Divider slot.
/// 子标题插槽。
/// Subheader slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VTreeviewStructuralItemSlotContext : VueProps
{
    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VuetifyItemProps? Props { get; init; }
}
