using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
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
/// Collection accepted by VDataIterator's items prop.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataIteratorItemsCollectionBuilder), nameof(VuetifyDataIteratorItemsCollectionBuilder.Create))]
public readonly struct VuetifyDataIteratorItems : IEnumerable<VuetifyDataIteratorItem>
{
    private readonly VuetifyDataIteratorItem[]? _items;

    private VuetifyDataIteratorItems(VuetifyDataIteratorItem[] items)
    {
        _items = items;
    }

    public VuetifyDataIteratorItem[]? AsArray => _items;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataIteratorItems From(VuetifyDataIteratorItem[] items);

    public static implicit operator VuetifyDataIteratorItems(VuetifyDataIteratorItem[] items)
        => new(items);

    IEnumerator<VuetifyDataIteratorItem> IEnumerable<VuetifyDataIteratorItem>.GetEnumerator()
        => ((IEnumerable<VuetifyDataIteratorItem>)(_items ?? [])).GetEnumerator();

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
/// Selected-value collection used by VDataIterator's modelValue prop.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataIteratorSelectedValuesCollectionBuilder), nameof(VuetifyDataIteratorSelectedValuesCollectionBuilder.Create))]
public readonly struct VuetifyDataIteratorSelectedValues : IEnumerable<VueValue>
{
    private readonly VueValue[]? _values;

    private VuetifyDataIteratorSelectedValues(VueValue[] values)
    {
        _values = values;
    }

    public VueValue[]? AsArray => _values;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataIteratorSelectedValues From(VueValue[] values);

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
        => ((IEnumerable<VueValue>)(_values ?? [])).GetEnumerator();

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
/// Per-key sort callbacks accepted by VDataIterator's customKeySort prop.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyDataIteratorSortFunctions : VueDictionary<VuetifyDataIteratorCompareFunction>;

/// <summary>
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
/// Union-like grouped item value used by VDataIterator slot contexts.
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct VuetifyDataIteratorGroupedItem
{
    private readonly byte _kind;
    private readonly VuetifyDataIteratorInternalItem? _item;
    private readonly VuetifyDataIteratorGroup? _group;

    private VuetifyDataIteratorGroupedItem(VuetifyDataIteratorInternalItem value)
    {
        _kind = 1;
        _item = value;
        _group = default;
    }

    private VuetifyDataIteratorGroupedItem(VuetifyDataIteratorGroup value)
    {
        _kind = 2;
        _item = default;
        _group = value;
    }

    public VuetifyDataIteratorInternalItem? AsItem => _kind == 1 ? _item : default;

    public VuetifyDataIteratorGroup? AsGroup => _kind == 2 ? _group : default;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataIteratorGroupedItem From(VuetifyDataIteratorInternalItem value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifyDataIteratorGroupedItem From(VuetifyDataIteratorGroup value);

    public static implicit operator VuetifyDataIteratorGroupedItem(VuetifyDataIteratorInternalItem value)
        => new(value);

    public static implicit operator VuetifyDataIteratorGroupedItem(VuetifyDataIteratorGroup value)
        => new(value);
}

/// <summary>
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
