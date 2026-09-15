using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VDataIterator 接受的数据项。任意项键作为普通对象成员发射。
/// Data item accepted by Vuetify VDataIterator. Arbitrary item keys are emitted as plain object members.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataIteratorItem : IEnumerable
{
    /// <summary>
    /// 按字段名读取或写入原始数据项的业务值。
    /// </summary>
    public extern VueValue? this[string key] { get; set; }

    /// <summary>
    /// 按字段名写入原始数据项，支持 C# 集合初始化器；同名字段使用新值覆盖。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <param name="key">字段名称；按该名称写入当前数据项的属性。</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern void Add(string key, VueValue value);

    extern IEnumerator IEnumerable.GetEnumerator();
}

/// <summary>
/// VDataIterator 的 items 属性接受的集合。
/// Collection accepted by VDataIterator's items prop.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataIteratorItemsCollectionBuilder), nameof(VuetifyDataIteratorItemsCollectionBuilder.Create))]
public readonly union VuetifyDataIteratorItems(VuetifyDataIteratorItem[]) : IEnumerable<VuetifyDataIteratorItem>
{
    /// <summary>
    /// 读取当前值的 VuetifyDataIteratorItem[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDataIteratorItem[]? AsArray => Value as VuetifyDataIteratorItem[];

    /// <summary>
    /// 将 VuetifyDataIteratorItem[] 值转换为 VuetifyDataIteratorItems，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataIteratorItems(VuetifyDataIteratorItem[] items)
        => new(items);

    IEnumerator<VuetifyDataIteratorItem> IEnumerable<VuetifyDataIteratorItem>.GetEnumerator()
        => ((IEnumerable<VuetifyDataIteratorItem>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataIteratorItem>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataIteratorItemsCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    public static VuetifyDataIteratorItems Create(ReadOnlySpan<VuetifyDataIteratorItem> items)
        => items.ToArray();
}

/// <summary>
/// VDataIterator 的 modelValue 属性使用的选中值集合。
/// Selected-value collection used by VDataIterator's modelValue prop.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataIteratorSelectedValuesCollectionBuilder), nameof(VuetifyDataIteratorSelectedValuesCollectionBuilder.Create))]
public readonly union VuetifyDataIteratorSelectedValues(VueValue[]) : IEnumerable<VueValue>
{
    /// <summary>
    /// 读取当前值的 VueValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VueValue[]? AsArray => Value as VueValue[];

    /// <summary>
    /// 将 VueValue[] 值转换为 VuetifyDataIteratorSelectedValues，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataIteratorSelectedValues(VueValue[] values)
        => new(values);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataIteratorSelectedValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 Number[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataIteratorSelectedValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 int[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataIteratorSelectedValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    /// <summary>
    /// 将 double[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataIteratorSelectedValues(double[] values)
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
public static class VuetifyDataIteratorSelectedValuesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifyDataIteratorSelectedValues Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

/// <summary>
/// 用于 VDataIterator.ValueComparator 的回调签名。
/// Apply a custom comparison algorithm to compare **model-value** and values contains in the **items** prop.
/// </summary>
public delegate bool VuetifyDataIteratorValueComparator(VueValue? first, VueValue? second);

/// <summary>
/// 比较两个数据值以排序；返回负数、零、正数分别表示左值在前、相等、右值在前。
/// </summary>
public delegate Number? VuetifyDataIteratorCompareFunction(VueValue? first, VueValue? second);

/// <summary>
/// VDataIterator 的 customKeySort 属性接受的按键排序回调集合。
/// Per-key sort callbacks accepted by VDataIterator's customKeySort prop.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyDataIteratorSortFunctions : VueDictionary<VuetifyDataIteratorCompareFunction>;

/// <summary>
/// 通过 VDataIterator 作用域插槽公开的内部项结构。
/// Internal item shape exposed through VDataIterator scoped slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyDataIteratorInternalItem
{
    /// <summary>
    /// 内部条目的类别标记，用于区分普通条目、分组等数据形状。
    /// </summary>
    [Description("@#type")]
    public string? Type { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public VueValue? Value { get; init; }

    /// <summary>
    /// 是否允许选择此数据项。
    /// </summary>
    [Description("@#selectable")]
    public bool Selectable { get; init; }

    /// <summary>
    /// 调用方提供的原始数据，供自定义渲染读取业务字段。
    /// </summary>
    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

/// <summary>
/// 通过 VDataIterator 的 groupedItems 插槽字段公开的分组节点结构。
/// Group node shape exposed through VDataIterator's groupedItems slot field.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyDataIteratorGroup
{
    /// <summary>
    /// 内部条目的类别标记，用于区分普通条目、分组等数据形状。
    /// </summary>
    [Description("@#type")]
    public string? Type { get; init; }

    /// <summary>
    /// 当前分组的嵌套层级，从最外层向内递增。
    /// </summary>
    [Description("@#depth")]
    public int Depth { get; init; }

    /// <summary>
    /// 当前条目的标识，用于组件内部关联及状态更新。
    /// </summary>
    [Description("@#id")]
    public string? Id { get; init; }

    /// <summary>
    /// 本分组所依据的数据字段键。
    /// </summary>
    [Description("@#key")]
    public string? Key { get; init; }

    /// <summary>
    /// 此条目关联的模型值，用于渲染和更新回调。
    /// </summary>
    [Description("@#value")]
    public VueValue? Value { get; init; }

    /// <summary>
    /// 当前组件处理后的条目集合，顺序与当前渲染顺序一致。
    /// </summary>
    [Description("@#items")]
    public VuetifyDataIteratorGroupedItem[]? Items { get; init; }
}

/// <summary>
/// VDataIterator 插槽上下文使用的类联合分组项值。
/// Union-like grouped item value used by VDataIterator slot contexts.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyDataIteratorGroupedItem(
    VuetifyDataIteratorInternalItem,
    VuetifyDataIteratorGroup)
{
    /// <summary>
    /// 读取当前值的 VuetifyDataIteratorInternalItem 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDataIteratorInternalItem? AsItem => Value as VuetifyDataIteratorInternalItem;

    /// <summary>
    /// 读取当前值的 VuetifyDataIteratorGroup 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifyDataIteratorGroup? AsGroup => Value as VuetifyDataIteratorGroup;

    /// <summary>
    /// 将 VuetifyDataIteratorInternalItem 值转换为 VuetifyDataIteratorGroupedItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataIteratorGroupedItem(VuetifyDataIteratorInternalItem value)
        => new(value);

    /// <summary>
    /// 将 VuetifyDataIteratorGroup 值转换为 VuetifyDataIteratorGroupedItem，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifyDataIteratorGroupedItem(VuetifyDataIteratorGroup value)
        => new(value);
}

/// <summary>
/// Vuetify VDataIterator 公开的默认/页眉/页脚插槽上下文。
/// Default/header/footer slot context exposed by Vuetify VDataIterator.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDataIteratorSlotContext
{
    /// <summary>
    /// 当前页码，从 1 开始。
    /// </summary>
    [Description("@#page")]
    public int Page { get; init; }

    /// <summary>
    /// 每页显示的条目数；-1 表示显示全部条目。
    /// </summary>
    [Description("@#itemsPerPage")]
    public int ItemsPerPage { get; init; }

    /// <summary>
    /// 当前排序字段和各字段的排序方向，数组顺序表示排序优先级。
    /// </summary>
    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    /// <summary>
    /// 根据当前总条目数与每页数量计算的总页数。
    /// </summary>
    [Description("@#pageCount")]
    public int PageCount { get; init; }

    /// <summary>
    /// 当前组件处理后的条目集合，顺序与当前渲染顺序一致。
    /// </summary>
    [Description("@#items")]
    public VuetifyDataIteratorInternalItem[]? Items { get; init; }

    /// <summary>
    /// 按 groupBy 组织后的分组条目。
    /// </summary>
    [Description("@#groupedItems")]
    public VuetifyDataIteratorGroupedItem[]? GroupedItems { get; init; }

    /// <summary>
    /// 切换指定列的排序方向，并更新当前排序状态。
    /// </summary>
    [Description("@#toggleSort")]
    public VDataIteratorToggleSortCallback? ToggleSort { get; init; }

    /// <summary>
    /// 切换到上一页。
    /// </summary>
    [Description("@#prevPage")]
    public VDataIteratorNavigationCallback? PrevPage { get; init; }

    /// <summary>
    /// 切换到下一页。
    /// </summary>
    [Description("@#nextPage")]
    public VDataIteratorNavigationCallback? NextPage { get; init; }

    /// <summary>
    /// 设置当前页码，页码从 1 开始。
    /// </summary>
    [Description("@#setPage")]
    public VDataIteratorSetPageCallback? SetPage { get; init; }

    /// <summary>
    /// 设置每页条目数并更新分页状态。
    /// </summary>
    [Description("@#setItemsPerPage")]
    public VDataIteratorSetItemsPerPageCallback? SetItemsPerPage { get; init; }

    /// <summary>
    /// 检查指定条目是否已选中的回调。
    /// </summary>
    [Description("@#isSelected")]
    public VDataIteratorIsSelectedCallback? IsSelected { get; init; }

    /// <summary>
    /// 根据传入的目标和布尔值更新选择状态。
    /// </summary>
    [Description("@#select")]
    public VDataIteratorSelectCallback? Select { get; init; }

    /// <summary>
    /// 设置当前可选择条目的全选状态。
    /// </summary>
    [Description("@#selectAll")]
    public VDataIteratorSelectAllCallback? SelectAll { get; init; }

    /// <summary>
    /// 切换指定条目的选中状态。
    /// </summary>
    [Description("@#toggleSelect")]
    public VDataIteratorToggleSelectCallback? ToggleSelect { get; init; }

    /// <summary>
    /// 检查指定条目的详情行是否已展开。
    /// </summary>
    [Description("@#isExpanded")]
    public VDataIteratorIsExpandedCallback? IsExpanded { get; init; }

    /// <summary>
    /// 切换指定条目的详情行展开状态。
    /// </summary>
    [Description("@#toggleExpand")]
    public VDataIteratorToggleExpandCallback? ToggleExpand { get; init; }

    /// <summary>
    /// 检查指定分组是否已展开。
    /// </summary>
    [Description("@#isGroupOpen")]
    public VDataIteratorIsGroupOpenCallback? IsGroupOpen { get; init; }

    /// <summary>
    /// 切换指定分组的展开状态。
    /// </summary>
    [Description("@#toggleGroup")]
    public VDataIteratorToggleGroupCallback? ToggleGroup { get; init; }
}

/// <summary>
/// 用于 VDataIteratorSlotContext.ToggleSort 的回调签名。
/// 切换指定列的排序方向，并更新当前排序状态。
/// </summary>
public delegate void VDataIteratorToggleSortCallback(VuetifyDataTableHeader header);

/// <summary>
/// 用于 VDataIteratorSlotContext.PrevPage、VDataIteratorSlotContext.NextPage 的回调签名。
/// 切换到上一页。
/// 切换到下一页。
/// </summary>
public delegate void VDataIteratorNavigationCallback();

/// <summary>
/// 用于 VDataIteratorSlotContext.SetPage 的回调签名。
/// 设置当前页码，页码从 1 开始。
/// </summary>
public delegate void VDataIteratorSetPageCallback(int page);

/// <summary>
/// 用于 VDataIteratorSlotContext.SetItemsPerPage 的回调签名。
/// 设置每页条目数并更新分页状态。
/// </summary>
public delegate void VDataIteratorSetItemsPerPageCallback(int itemsPerPage);

/// <summary>
/// 用于 VDataIteratorSlotContext.IsSelected 的回调签名。
/// 检查指定条目是否已选中的回调。
/// </summary>
public delegate bool VDataIteratorIsSelectedCallback(VuetifyDataIteratorInternalItem item);

/// <summary>
/// 用于 VDataIteratorSlotContext.Select 的回调签名。
/// 根据传入的目标和布尔值更新选择状态。
/// </summary>
/// <param name="items">按期望顺序排列的元素。</param>
/// <param name="selected">true 选中传入的项目，false 取消选中。</param>
public delegate void VDataIteratorSelectCallback(VuetifyDataIteratorInternalItem[] items, bool selected);

/// <summary>
/// 用于 VDataIteratorSlotContext.SelectAll 的回调签名。
/// 设置当前可选择条目的全选状态。
/// </summary>
public delegate void VDataIteratorSelectAllCallback(bool selected);

/// <summary>
/// 用于 VDataIteratorSlotContext.ToggleSelect 的回调签名。
/// 切换指定条目的选中状态。
/// </summary>
public delegate void VDataIteratorToggleSelectCallback(VuetifyDataIteratorInternalItem item);

/// <summary>
/// 用于 VDataIteratorSlotContext.IsExpanded 的回调签名。
/// 检查指定条目的详情行是否已展开。
/// </summary>
public delegate bool VDataIteratorIsExpandedCallback(VuetifyDataIteratorInternalItem item);

/// <summary>
/// 用于 VDataIteratorSlotContext.ToggleExpand 的回调签名。
/// 切换指定条目的详情行展开状态。
/// </summary>
public delegate void VDataIteratorToggleExpandCallback(VuetifyDataIteratorInternalItem item);

/// <summary>
/// 用于 VDataIteratorSlotContext.IsGroupOpen 的回调签名。
/// 检查指定分组是否已展开。
/// </summary>
public delegate bool VDataIteratorIsGroupOpenCallback(VuetifyDataIteratorGroup group);

/// <summary>
/// 用于 VDataIteratorSlotContext.ToggleGroup 的回调签名。
/// 切换指定分组的展开状态。
/// </summary>
public delegate void VDataIteratorToggleGroupCallback(VuetifyDataIteratorGroup group);