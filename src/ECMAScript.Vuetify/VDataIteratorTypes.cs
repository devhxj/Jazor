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
    public extern VueValue? this[string key] { get; set; }

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
    public VuetifyDataIteratorItem[]? AsArray => Value as VuetifyDataIteratorItem[];

    public static implicit operator VuetifyDataIteratorItems(VuetifyDataIteratorItem[] items)
        => new(items);

    IEnumerator<VuetifyDataIteratorItem> IEnumerable<VuetifyDataIteratorItem>.GetEnumerator()
        => ((IEnumerable<VuetifyDataIteratorItem>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataIteratorItem>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataIteratorItemsCollectionBuilder
{
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
    public VueValue[]? AsArray => Value as VueValue[];

    public static implicit operator VuetifyDataIteratorSelectedValues(VueValue[] values)
        => new(values);

    public static implicit operator VuetifyDataIteratorSelectedValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDataIteratorSelectedValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDataIteratorSelectedValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDataIteratorSelectedValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataIteratorSelectedValuesCollectionBuilder
{
    public static VuetifyDataIteratorSelectedValues Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

public delegate bool VuetifyDataIteratorValueComparator(VueValue? first, VueValue? second);

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
    [Description("@#type")]
    public string? Type { get; init; }

    [Description("@#value")]
    public VueValue? Value { get; init; }

    [Description("@#selectable")]
    public bool Selectable { get; init; }

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
    [Description("@#type")]
    public string? Type { get; init; }

    [Description("@#depth")]
    public int Depth { get; init; }

    [Description("@#id")]
    public string? Id { get; init; }

    [Description("@#key")]
    public string? Key { get; init; }

    [Description("@#value")]
    public VueValue? Value { get; init; }

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
    public VuetifyDataIteratorInternalItem? AsItem => Value as VuetifyDataIteratorInternalItem;

    public VuetifyDataIteratorGroup? AsGroup => Value as VuetifyDataIteratorGroup;

    public static implicit operator VuetifyDataIteratorGroupedItem(VuetifyDataIteratorInternalItem value)
        => new(value);

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
    [Description("@#page")]
    public int Page { get; init; }

    [Description("@#itemsPerPage")]
    public int ItemsPerPage { get; init; }

    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    [Description("@#pageCount")]
    public int PageCount { get; init; }

    [Description("@#items")]
    public VuetifyDataIteratorInternalItem[]? Items { get; init; }

    [Description("@#groupedItems")]
    public VuetifyDataIteratorGroupedItem[]? GroupedItems { get; init; }

    [Description("@#toggleSort")]
    public VDataIteratorToggleSortCallback? ToggleSort { get; init; }

    [Description("@#prevPage")]
    public VDataIteratorNavigationCallback? PrevPage { get; init; }

    [Description("@#nextPage")]
    public VDataIteratorNavigationCallback? NextPage { get; init; }

    [Description("@#setPage")]
    public VDataIteratorSetPageCallback? SetPage { get; init; }

    [Description("@#setItemsPerPage")]
    public VDataIteratorSetItemsPerPageCallback? SetItemsPerPage { get; init; }

    [Description("@#isSelected")]
    public VDataIteratorIsSelectedCallback? IsSelected { get; init; }

    [Description("@#select")]
    public VDataIteratorSelectCallback? Select { get; init; }

    [Description("@#selectAll")]
    public VDataIteratorSelectAllCallback? SelectAll { get; init; }

    [Description("@#toggleSelect")]
    public VDataIteratorToggleSelectCallback? ToggleSelect { get; init; }

    [Description("@#isExpanded")]
    public VDataIteratorIsExpandedCallback? IsExpanded { get; init; }

    [Description("@#toggleExpand")]
    public VDataIteratorToggleExpandCallback? ToggleExpand { get; init; }

    [Description("@#isGroupOpen")]
    public VDataIteratorIsGroupOpenCallback? IsGroupOpen { get; init; }

    [Description("@#toggleGroup")]
    public VDataIteratorToggleGroupCallback? ToggleGroup { get; init; }
}

public delegate void VDataIteratorToggleSortCallback(VuetifyDataTableHeader header);

public delegate void VDataIteratorNavigationCallback();

public delegate void VDataIteratorSetPageCallback(int page);

public delegate void VDataIteratorSetItemsPerPageCallback(int itemsPerPage);

public delegate bool VDataIteratorIsSelectedCallback(VuetifyDataIteratorInternalItem item);

public delegate void VDataIteratorSelectCallback(VuetifyDataIteratorInternalItem[] items, bool selected);

public delegate void VDataIteratorSelectAllCallback(bool selected);

public delegate void VDataIteratorToggleSelectCallback(VuetifyDataIteratorInternalItem item);

public delegate bool VDataIteratorIsExpandedCallback(VuetifyDataIteratorInternalItem item);

public delegate void VDataIteratorToggleExpandCallback(VuetifyDataIteratorInternalItem item);

public delegate bool VDataIteratorIsGroupOpenCallback(VuetifyDataIteratorGroup group);

public delegate void VDataIteratorToggleGroupCallback(VuetifyDataIteratorGroup group);
