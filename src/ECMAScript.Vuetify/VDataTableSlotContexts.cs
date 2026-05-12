namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 数据表格结构插槽公开的共享状态。
/// Shared state exposed by Vuetify data table structural slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDataTableSlotContext
{
    [Description("@#page")]
    public int Page { get; init; }

    [Description("@#itemsPerPage")]
    public int ItemsPerPage { get; init; }

    [Description("@#pageCount")]
    public int PageCount { get; init; }

    [Description("@#itemsLength")]
    public int ItemsLength { get; init; }

    [Description("@#headers")]
    public VuetifyDataTableHeader[][]? Headers { get; init; }

    [Description("@#columns")]
    public VuetifyDataTableHeader[]? Columns { get; init; }

    [Description("@#items")]
    public VuetifyDataTableItem[]? Items { get; init; }

    [Description("@#internalItems")]
    public VuetifyDataTableItem[]? InternalItems { get; init; }

    [Description("@#groupedItems")]
    public VuetifyDataTableItem[]? GroupedItems { get; init; }

    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    [Description("@#setItemsPerPage")]
    public VDataTableSetItemsPerPageCallback? SetItemsPerPage { get; init; }

    [Description("@#toggleSort")]
    public VDataTableToggleSortCallback? ToggleSort { get; init; }

    [Description("@#isSorted")]
    public VDataTableIsSortedCallback? IsSorted { get; init; }
}

/// <summary>
/// Vuetify 数据表格头部插槽上下文。
/// Vuetify data table headers slot context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDataTableHeadersSlotContext
{
    [Description("@#headers")]
    public VuetifyDataTableHeader[][]? Headers { get; init; }

    [Description("@#columns")]
    public VuetifyDataTableHeader[]? Columns { get; init; }

    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    [Description("@#someSelected")]
    public bool SomeSelected { get; init; }

    [Description("@#allSelected")]
    public bool AllSelected { get; init; }

    [Description("@#selectAll")]
    public VDataTableSelectAllCallback? SelectAll { get; init; }

    [Description("@#toggleSort")]
    public VDataTableToggleSortCallback? ToggleSort { get; init; }

    [Description("@#isSorted")]
    public VDataTableIsSortedCallback? IsSorted { get; init; }
}

/// <summary>
/// Vuetify 数据表格头部单元格插槽上下文。
/// Vuetify data table header cell slot context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDataTableHeaderCellSlotContext
{
    [Description("@#column")]
    public VuetifyDataTableHeader? Column { get; init; }

    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    [Description("@#someSelected")]
    public bool SomeSelected { get; init; }

    [Description("@#allSelected")]
    public bool AllSelected { get; init; }

    [Description("@#selectAll")]
    public VDataTableSelectAllCallback? SelectAll { get; init; }

    [Description("@#toggleSort")]
    public VDataTableToggleSortCallback? ToggleSort { get; init; }

    [Description("@#isSorted")]
    public VDataTableIsSortedCallback? IsSorted { get; init; }
}

/// <summary>
/// Vuetify 数据表格项插槽上下文。
/// Vuetify data table item slot context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDataTableItemSlotContext
{
    [Description("@#item")]
    public VuetifyDataTableItem? Item { get; init; }

    [Description("@#internalItem")]
    public VuetifyDataTableItem? InternalItem { get; init; }

    [Description("@#columns")]
    public VuetifyDataTableHeader[]? Columns { get; init; }

    [Description("@#isSelected")]
    public VDataTableIsSelectedCallback? IsSelected { get; init; }

    [Description("@#toggleSelect")]
    public VDataTableToggleSelectCallback? ToggleSelect { get; init; }

    [Description("@#isExpanded")]
    public VDataTableIsExpandedCallback? IsExpanded { get; init; }

    [Description("@#toggleExpand")]
    public VDataTableToggleExpandCallback? ToggleExpand { get; init; }
}

/// <summary>
/// Vuetify 数据表格分组头部插槽上下文。
/// Vuetify data table group header slot context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDataTableGroupHeaderSlotContext
{
    [Description("@#item")]
    public VuetifyDataTableItem? Item { get; init; }

    [Description("@#columns")]
    public VuetifyDataTableHeader[]? Columns { get; init; }

    [Description("@#isGroupOpen")]
    public VDataTableIsGroupOpenCallback? IsGroupOpen { get; init; }

    [Description("@#toggleGroup")]
    public VDataTableToggleGroupCallback? ToggleGroup { get; init; }
}

/// <summary>
/// Vuetify 数据表格行属性上下文。
/// Vuetify data table row props context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyDataTableRowPropsContext
{
    [Description("@#index")]
    public int Index { get; init; }

    [Description("@#item")]
    public VuetifyDataTableItem? Item { get; init; }

    [Description("@#internalItem")]
    public VuetifyDataTableItem? InternalItem { get; init; }
}

/// <summary>
/// Vuetify 数据表格单元格属性上下文。
/// Vuetify data table cell props context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyDataTableCellPropsContext
{
    [Description("@#index")]
    public int Index { get; init; }

    [Description("@#item")]
    public VuetifyDataTableItem? Item { get; init; }

    [Description("@#internalItem")]
    public VuetifyDataTableItem? InternalItem { get; init; }

    [Description("@#column")]
    public VuetifyDataTableHeader? Column { get; init; }
}

public delegate void VDataTableSetItemsPerPageCallback(int itemsPerPage);

public delegate void VDataTableToggleSortCallback(VuetifyDataTableHeader header);

public delegate bool VDataTableIsSortedCallback(VuetifyDataTableHeader header);

public delegate void VDataTableSelectAllCallback(bool selected);

public delegate bool VDataTableIsSelectedCallback(VuetifyDataTableItem item);

public delegate void VDataTableToggleSelectCallback(VuetifyDataTableItem item);

public delegate bool VDataTableIsExpandedCallback(VuetifyDataTableItem item);

public delegate void VDataTableToggleExpandCallback(VuetifyDataTableItem item);

public delegate bool VDataTableIsGroupOpenCallback(VuetifyDataTableItem item);

public delegate void VDataTableToggleGroupCallback(VuetifyDataTableItem item);
