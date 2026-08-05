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
    public VuetifySelectItemValue[]? AsArray
        => Value is VuetifySelectItemValue[] value ? value : default(VuetifySelectItemValue[]?);

    public static implicit operator VuetifySelectItems(VuetifySelectItemValue[] items)
        => new(items);

    public static implicit operator VuetifySelectItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySelectItemValue)item));

    IEnumerator<VuetifySelectItemValue> IEnumerable<VuetifySelectItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifySelectItemValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySelectItemValue>)this).GetEnumerator();
}


[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySelectItemsCollectionBuilder
{
    public static VuetifySelectItems Create(ReadOnlySpan<VuetifySelectItemValue> items)
        => items.ToArray();
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySelectModelValuesCollectionBuilder), nameof(VuetifySelectModelValuesCollectionBuilder.Create))]
public readonly union VuetifySelectModelValues(VuetifySelectModelValue[]) : IEnumerable<VuetifySelectModelValue>
{
    public VuetifySelectModelValue[]? AsArray
        => Value is VuetifySelectModelValue[] value ? value : default(VuetifySelectModelValue[]?);

    public static implicit operator VuetifySelectModelValues(VuetifySelectModelValue[] values)
        => new(values);

    public static implicit operator VuetifySelectModelValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(bool[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(Symbol[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(VueProps[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(VueDictionary[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    public static implicit operator VuetifySelectModelValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySelectModelValue)value));

    IEnumerator<VuetifySelectModelValue> IEnumerable<VuetifySelectModelValue>.GetEnumerator()
        => ((IEnumerable<VuetifySelectModelValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySelectModelValue>)this).GetEnumerator();
}


[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySelectModelValuesCollectionBuilder
{
    public static VuetifySelectModelValues Create(ReadOnlySpan<VuetifySelectModelValue> values)
        => values.ToArray();
}

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
    public string? AsString
        => Value is string value ? value : default(string?);

    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public Symbol? AsSymbol
        => Value is Symbol value ? value : default(Symbol?);

    public VueProps? AsObject
        => Value is VueProps value ? value : default(VueProps?);

    public VuetifySelectModelValues? AsValues
        => Value is VuetifySelectModelValues value ? value : default(VuetifySelectModelValues?);

    public static implicit operator VuetifySelectModelValue(string value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(Number value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(bool value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(Symbol value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(VueProps value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(VueDictionary value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(VuetifySelectModelValues value)
        => new(value);

    public static implicit operator VuetifySelectModelValue(VuetifySelectModelValue[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(string[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(Number[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(bool[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(Symbol[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(VueProps[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(VueDictionary[] value)
        => new((VuetifySelectModelValues)value);

    public static implicit operator VuetifySelectModelValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(short value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(int value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(float value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(double value)
        => new((Number)value);

    public static implicit operator VuetifySelectModelValue(decimal value)
        => new((Number)value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifySelectItemValue(
    string,
    VuetifySelectItem,
    Number,
    bool,
    VueProps)
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public VuetifySelectItem? AsItem
        => Value is VuetifySelectItem value ? value : default(VuetifySelectItem?);

    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public VueProps? AsObject
        => Value is VueProps value ? value : default(VueProps?);

    public static implicit operator VuetifySelectItemValue(string value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(VuetifySelectItem value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(Number value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(bool value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(VueProps value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(VueDictionary value)
        => new(value);

    public static implicit operator VuetifySelectItemValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(short value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(int value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(float value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(double value)
        => new((Number)value);

    public static implicit operator VuetifySelectItemValue(decimal value)
        => new((Number)value);
}


[ECMAScript]
[Description("@#")]
public sealed class VuetifySelectItem
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#value")]
    public VueValue? Value { get; init; }

    [Description("@#props")]
    public VuetifySelectItemPropsValue? Props { get; init; }

    [Description("@#children")]
    public VuetifySelectItemValue[]? Children { get; init; }

    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifySelectItemKey(
    string,
    string[],
    VuetifySelectItemKeySelector,
    bool)
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public string[]? AsPath
        => Value is string[] value ? value : default(string[]?);

    public VuetifySelectItemKeySelector? AsSelector
        => Value is VuetifySelectItemKeySelector value ? value : default(VuetifySelectItemKeySelector?);

    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public static implicit operator VuetifySelectItemKey(string value)
        => new(value);

    public static implicit operator VuetifySelectItemKey(string[] value)
        => new(value);

    public static implicit operator VuetifySelectItemKey(VuetifySelectItemKeySelector value)
        => new(value);

    public static implicit operator VuetifySelectItemKey(bool value)
        => new(value);
}


public delegate VueValue? VuetifySelectItemKeySelector(VueValue item, string fallback);

[ECMAScript]
[Description("@#")]
public readonly union VuetifySelectItemPropsSelector(
    string,
    string[],
    VuetifySelectItemPropsCallback,
    bool)
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public string[]? AsPath
        => Value is string[] value ? value : default(string[]?);

    public VuetifySelectItemPropsCallback? AsCallback
        => Value is VuetifySelectItemPropsCallback value ? value : default(VuetifySelectItemPropsCallback?);

    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public static implicit operator VuetifySelectItemPropsSelector(string value)
        => new(value);

    public static implicit operator VuetifySelectItemPropsSelector(string[] value)
        => new(value);

    public static implicit operator VuetifySelectItemPropsSelector(VuetifySelectItemPropsCallback value)
        => new(value);

    public static implicit operator VuetifySelectItemPropsSelector(bool value)
        => new(value);
}


public delegate VuetifyItemProps? VuetifySelectItemPropsCallback(VueValue item);

public delegate bool VuetifySelectValueComparator(VueValue? first, VueValue? second);

public delegate VuetifyFilterMatch VuetifyFilterFunction(string? value, string? query, VuetifyListItem? item = default);

[ECMAScript]
[Description("@#")]
public sealed record VuetifyFilterKeyFunctions : VueDictionary<VuetifyFilterFunction>;

[String]
public enum VuetifyFilterMode
{
    [Description("@#some")]
    Some,

    [Description("@#every")]
    Every,

    [Description("@#union")]
    Union,

    [Description("@#intersection")]
    Intersection
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyFilterKeys(string, string[])
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public string[]? AsStrings
        => Value is string[] value ? value : default(string[]?);

    public static implicit operator VuetifyFilterKeys(string value)
        => new(value);

    public static implicit operator VuetifyFilterKeys(string[] value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public readonly union VuetifyFilterMatch(
    bool,
    Number,
    Number[],
    Number[][])
{
    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public Number[]? AsRange
        => Value is Number[] value ? value : default(Number[]?);

    public Number[][]? AsRanges
        => Value is Number[][] value ? value : default(Number[][]?);

    [ECMAScriptInline("[__arg1, __arg2]")]
    public extern static VuetifyFilterMatch Range(Number start, Number end);

    public static implicit operator VuetifyFilterMatch(bool value)
        => new(value);

    public static implicit operator VuetifyFilterMatch(Number value)
        => new(value);

    public static implicit operator VuetifyFilterMatch(Number[] value)
        => new(value);

    public static implicit operator VuetifyFilterMatch(Number[][] value)
        => new(value);

    public static implicit operator VuetifyFilterMatch(byte value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(short value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(int value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(uint value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(float value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(double value)
        => new((Number)value);

    public static implicit operator VuetifyFilterMatch(decimal value)
        => new((Number)value);
}


[ECMAScript]
[Description("@#")]
public sealed record VuetifyListItem
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#value")]
    public VueValue? Value { get; init; }

    [Description("@#props")]
    public VueProps? Props { get; init; }

    [Description("@#children")]
    public VuetifyListItem[]? Children { get; init; }

    [Description("@#raw")]
    public VueValue? Raw { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifySelectItemPropsValue(VuetifyItemProps, bool)
{
    public VuetifyItemProps? AsProps
        => Value is VuetifyItemProps value ? value : default(VuetifyItemProps?);

    public bool? AsBool
        => Value is bool value ? value : default(bool?);

    public static implicit operator VuetifySelectItemPropsValue(VuetifyItemProps value)
        => new(value);

    public static implicit operator VuetifySelectItemPropsValue(bool value)
        => new(value);
}


[ECMAScript]
[Description("@#")]
public sealed class VuetifyItemProps : IEnumerable
{
    public extern VueValue? this[string key] { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern void Add(string key, VueValue value);

    extern IEnumerator IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyBreadcrumbItemsCollectionBuilder), nameof(VuetifyBreadcrumbItemsCollectionBuilder.Create))]
public readonly union VuetifyBreadcrumbItems(VuetifyBreadcrumbItemValue[]) : IEnumerable<VuetifyBreadcrumbItemValue>
{
    public VuetifyBreadcrumbItemValue[]? AsArray
        => Value is VuetifyBreadcrumbItemValue[] value ? value : default(VuetifyBreadcrumbItemValue[]?);

    public static implicit operator VuetifyBreadcrumbItems(VuetifyBreadcrumbItemValue[] items)
        => new(items);

    public static implicit operator VuetifyBreadcrumbItems(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifyBreadcrumbItemValue)item));

    IEnumerator<VuetifyBreadcrumbItemValue> IEnumerable<VuetifyBreadcrumbItemValue>.GetEnumerator()
        => ((IEnumerable<VuetifyBreadcrumbItemValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyBreadcrumbItemValue>)this).GetEnumerator();
}


[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyBreadcrumbItemsCollectionBuilder
{
    public static VuetifyBreadcrumbItems Create(ReadOnlySpan<VuetifyBreadcrumbItemValue> items)
        => items.ToArray();
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyBreadcrumbItemValue(
    string,
    VuetifyBreadcrumbItem,
    Number)
{
    public string? AsString
        => Value is string value ? value : default(string?);

    public VuetifyBreadcrumbItem? AsItem
        => Value is VuetifyBreadcrumbItem value ? value : default(VuetifyBreadcrumbItem?);

    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public static implicit operator VuetifyBreadcrumbItemValue(string value)
        => new(value);

    public static implicit operator VuetifyBreadcrumbItemValue(VuetifyBreadcrumbItem value)
        => new(value);

    public static implicit operator VuetifyBreadcrumbItemValue(Number value)
        => new(value);

    public static implicit operator VuetifyBreadcrumbItemValue(byte value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(short value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(int value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(uint value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(float value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(double value)
        => new((Number)value);

    public static implicit operator VuetifyBreadcrumbItemValue(decimal value)
        => new((Number)value);
}


[ECMAScript]
[Description("@#")]
public sealed class VuetifyBreadcrumbItem
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    [Description("@#href")]
    public string? Href { get; init; }

    [Description("@#to")]
    public string? To { get; init; }

    [Description("@#replace")]
    public bool? Replace { get; init; }

    [Description("@#exact")]
    public bool? Exact { get; init; }
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableHeadersCollectionBuilder), nameof(VuetifyDataTableHeadersCollectionBuilder.Create))]
public readonly union VuetifyDataTableHeaders(VuetifyDataTableHeader[]) : IEnumerable<VuetifyDataTableHeader>
{
    public VuetifyDataTableHeader[]? AsArray
        => Value is VuetifyDataTableHeader[] value ? value : default(VuetifyDataTableHeader[]?);

    public static implicit operator VuetifyDataTableHeaders(VuetifyDataTableHeader[] headers)
        => new(headers);

    IEnumerator<VuetifyDataTableHeader> IEnumerable<VuetifyDataTableHeader>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableHeader>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableHeader>)this).GetEnumerator();
}


[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableHeadersCollectionBuilder
{
    public static VuetifyDataTableHeaders Create(ReadOnlySpan<VuetifyDataTableHeader> headers)
        => headers.ToArray();
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableHeader
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#key")]
    public string? Key { get; init; }

    [Description("@#value")]
    public VuetifySelectItemKey? Value { get; init; }

    [Description("@#sortable")]
    public bool? Sortable { get; init; }

    [Description("@#align")]
    public VuetifyDataTableHeaderAlign? Align { get; init; }

    [Description("@#width")]
    public VueStringNumberValue? Width { get; init; }

    [Description("@#minWidth")]
    public VueStringNumberValue? MinWidth { get; init; }

    [Description("@#maxWidth")]
    public VueStringNumberValue? MaxWidth { get; init; }

    [Description("@#nowrap")]
    public bool? Nowrap { get; init; }

    [Description("@#fixed")]
    public bool? Fixed { get; init; }

    [Description("@#children")]
    public VuetifyDataTableHeader[]? Children { get; init; }
}

[String]
public enum VuetifyDataTableHeaderAlign
{
    [Description("@#start")]
    Start,

    [Description("@#end")]
    End,

    [Description("@#center")]
    Center
}

[String]
public enum VuetifyDataTableSortOrder
{
    [Description("@#asc")]
    Asc,

    [Description("@#desc")]
    Desc
}

[String]
public enum VuetifyDataTableSelectStrategy
{
    [Description("@#single")]
    Single,

    [Description("@#page")]
    Page,

    [Description("@#all")]
    All
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableItemsCollectionBuilder), nameof(VuetifyDataTableItemsCollectionBuilder.Create))]
public readonly union VuetifyDataTableItems(VuetifyDataTableItem[]) : IEnumerable<VuetifyDataTableItem>
{
    public VuetifyDataTableItem[]? AsArray
        => Value is VuetifyDataTableItem[] value ? value : default(VuetifyDataTableItem[]?);

    public static implicit operator VuetifyDataTableItems(VuetifyDataTableItem[] items)
        => new(items);

    IEnumerator<VuetifyDataTableItem> IEnumerable<VuetifyDataTableItem>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItem>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItem>)this).GetEnumerator();
}


[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableItemsCollectionBuilder
{
    public static VuetifyDataTableItems Create(ReadOnlySpan<VuetifyDataTableItem> items)
        => items.ToArray();
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableItem : IEnumerable
{
    public extern VueValue? this[string key] { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public extern void Add(string key, VueValue value);

    extern IEnumerator IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableSelectedValuesCollectionBuilder), nameof(VuetifyDataTableSelectedValuesCollectionBuilder.Create))]
public readonly union VuetifyDataTableSelectedValues(VueValue[]) : IEnumerable<VueValue>
{
    public VueValue[]? AsArray
        => Value is VueValue[] value ? value : default(VueValue[]?);

    public static implicit operator VuetifyDataTableSelectedValues(VueValue[] values)
        => new(values);

    public static implicit operator VuetifyDataTableSelectedValues(string[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDataTableSelectedValues(Number[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDataTableSelectedValues(int[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    public static implicit operator VuetifyDataTableSelectedValues(double[] values)
        => new(Array.ConvertAll(values, static value => (VueValue)value));

    IEnumerator<VueValue> IEnumerable<VueValue>.GetEnumerator()
        => ((IEnumerable<VueValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VueValue>)this).GetEnumerator();
}


[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableSelectedValuesCollectionBuilder
{
    public static VuetifyDataTableSelectedValues Create(ReadOnlySpan<VueValue> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableSortItemsCollectionBuilder), nameof(VuetifyDataTableSortItemsCollectionBuilder.Create))]
public readonly union VuetifyDataTableSortItems(VuetifyDataTableSortItem[]) : IEnumerable<VuetifyDataTableSortItem>
{
    public VuetifyDataTableSortItem[]? AsArray
        => Value is VuetifyDataTableSortItem[] value ? value : default(VuetifyDataTableSortItem[]?);

    public static implicit operator VuetifyDataTableSortItems(VuetifyDataTableSortItem[] items)
        => new(items);

    IEnumerator<VuetifyDataTableSortItem> IEnumerable<VuetifyDataTableSortItem>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableSortItem>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableSortItem>)this).GetEnumerator();
}


[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableSortItemsCollectionBuilder
{
    public static VuetifyDataTableSortItems Create(ReadOnlySpan<VuetifyDataTableSortItem> items)
        => items.ToArray();
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableSortItem
{
    [Description("@#key")]
    public string? Key { get; init; }

    [Description("@#order")]
    public VuetifyDataTableSortOrder? Order { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableOptions
{
    [Description("@#page")]
    public int? Page { get; init; }

    [Description("@#itemsPerPage")]
    public int? ItemsPerPage { get; init; }

    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    [Description("@#groupBy")]
    public VuetifyDataTableSortItem[]? GroupBy { get; init; }

    [Description("@#search")]
    public string? Search { get; init; }
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifyDataTableItemsPerPageOptionsCollectionBuilder), nameof(VuetifyDataTableItemsPerPageOptionsCollectionBuilder.Create))]
public readonly union VuetifyDataTableItemsPerPageOptions(VuetifyDataTableItemsPerPageOption[]) : IEnumerable<VuetifyDataTableItemsPerPageOption>
{
    public VuetifyDataTableItemsPerPageOption[]? AsArray
        => Value is VuetifyDataTableItemsPerPageOption[] value ? value : default(VuetifyDataTableItemsPerPageOption[]?);

    public static implicit operator VuetifyDataTableItemsPerPageOptions(VuetifyDataTableItemsPerPageOption[] options)
        => new(options);

    public static implicit operator VuetifyDataTableItemsPerPageOptions(Number[] options)
        => new(Array.ConvertAll(options, static value => (VuetifyDataTableItemsPerPageOption)value));

    public static implicit operator VuetifyDataTableItemsPerPageOptions(int[] options)
        => new(Array.ConvertAll(options, static value => (VuetifyDataTableItemsPerPageOption)value));

    IEnumerator<VuetifyDataTableItemsPerPageOption> IEnumerable<VuetifyDataTableItemsPerPageOption>.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItemsPerPageOption>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifyDataTableItemsPerPageOption>)this).GetEnumerator();
}


[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifyDataTableItemsPerPageOptionsCollectionBuilder
{
    public static VuetifyDataTableItemsPerPageOptions Create(ReadOnlySpan<VuetifyDataTableItemsPerPageOption> options)
        => options.ToArray();
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyDataTableItemsPerPageOption(Number, VuetifyDataTableItemsPerPageOptionItem)
{
    public Number? AsNumber
        => Value is Number value ? value : default(Number?);

    public VuetifyDataTableItemsPerPageOptionItem? AsItem
        => Value is VuetifyDataTableItemsPerPageOptionItem value ? value : default(VuetifyDataTableItemsPerPageOptionItem?);

    public static implicit operator VuetifyDataTableItemsPerPageOption(Number value)
        => new(value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(VuetifyDataTableItemsPerPageOptionItem value)
        => new(value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(byte value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(sbyte value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(short value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(ushort value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(int value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(uint value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(float value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(double value)
        => new((Number)value);

    public static implicit operator VuetifyDataTableItemsPerPageOption(decimal value)
        => new((Number)value);
}


[ECMAScript]
[Description("@#")]
public sealed class VuetifyDataTableItemsPerPageOptionItem
{
    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#value")]
    public int? Value { get; init; }
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifyDataTableRowProps(VueProps, VuetifyDataTableRowPropsCallback)
{
    public VueProps? AsProps
        => Value is VueProps value ? value : default(VueProps?);

    public VuetifyDataTableRowPropsCallback? AsCallback
        => Value is VuetifyDataTableRowPropsCallback value ? value : default(VuetifyDataTableRowPropsCallback?);

    public static implicit operator VuetifyDataTableRowProps(VueProps value)
        => new(value);

    public static implicit operator VuetifyDataTableRowProps(VueDictionary value)
        => new(value);

    public static implicit operator VuetifyDataTableRowProps(VuetifyDataTableRowPropsCallback value)
        => new(value);
}


public delegate VueProps? VuetifyDataTableRowPropsCallback(VuetifyDataTableRowPropsContext context);

[ECMAScript]
[Description("@#")]
public readonly union VuetifyDataTableCellProps(VueProps, VuetifyDataTableCellPropsCallback)
{
    public VueProps? AsProps
        => Value is VueProps value ? value : default(VueProps?);

    public VuetifyDataTableCellPropsCallback? AsCallback
        => Value is VuetifyDataTableCellPropsCallback value ? value : default(VuetifyDataTableCellPropsCallback?);

    public static implicit operator VuetifyDataTableCellProps(VueProps value)
        => new(value);

    public static implicit operator VuetifyDataTableCellProps(VueDictionary value)
        => new(value);

    public static implicit operator VuetifyDataTableCellProps(VuetifyDataTableCellPropsCallback value)
        => new(value);
}


public delegate VueProps? VuetifyDataTableCellPropsCallback(VuetifyDataTableCellPropsContext context);
