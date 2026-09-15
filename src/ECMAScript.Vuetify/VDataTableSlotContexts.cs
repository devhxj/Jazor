namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 数据表格结构插槽公开的共享状态。
/// Shared state exposed by Vuetify data table structural slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VDataTableSlotContext
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
    /// 根据当前总条目数与每页数量计算的总页数。
    /// </summary>
    [Description("@#pageCount")]
    public int PageCount { get; init; }

    /// <summary>
    /// 用于分页的总条目数。
    /// </summary>
    [Description("@#itemsLength")]
    public int ItemsLength { get; init; }

    /// <summary>
    /// 按表头层级组织的列定义，每一层对应一行表头。
    /// </summary>
    [Description("@#headers")]
    public VuetifyDataTableHeader[][]? Headers { get; init; }

    /// <summary>
    /// 当前可渲染的叶子列定义，顺序与表格显示列一致。
    /// </summary>
    [Description("@#columns")]
    public VuetifyDataTableHeader[]? Columns { get; init; }

    /// <summary>
    /// 当前组件处理后的条目集合，顺序与当前渲染顺序一致。
    /// </summary>
    [Description("@#items")]
    public VuetifyDataTableItem[]? Items { get; init; }

    /// <summary>
    /// 当前内部条目集合，包含转换后的渲染和选择信息。
    /// </summary>
    [Description("@#internalItems")]
    public VuetifyDataTableItem[]? InternalItems { get; init; }

    /// <summary>
    /// 按 groupBy 组织后的分组条目。
    /// </summary>
    [Description("@#groupedItems")]
    public VuetifyDataTableItem[]? GroupedItems { get; init; }

    /// <summary>
    /// 当前排序字段和各字段的排序方向，数组顺序表示排序优先级。
    /// </summary>
    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    /// <summary>
    /// 设置每页条目数并更新分页状态。
    /// </summary>
    [Description("@#setItemsPerPage")]
    public VDataTableSetItemsPerPageCallback? SetItemsPerPage { get; init; }

    /// <summary>
    /// 切换指定列的排序方向，并更新当前排序状态。
    /// </summary>
    [Description("@#toggleSort")]
    public VDataTableToggleSortCallback? ToggleSort { get; init; }

    /// <summary>
    /// 检查指定列当前是否参与排序。
    /// </summary>
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
    /// <summary>
    /// 按表头层级组织的列定义，每一层对应一行表头。
    /// </summary>
    [Description("@#headers")]
    public VuetifyDataTableHeader[][]? Headers { get; init; }

    /// <summary>
    /// 当前可渲染的叶子列定义，顺序与表格显示列一致。
    /// </summary>
    [Description("@#columns")]
    public VuetifyDataTableHeader[]? Columns { get; init; }

    /// <summary>
    /// 当前排序字段和各字段的排序方向，数组顺序表示排序优先级。
    /// </summary>
    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    /// <summary>
    /// 当前条目集合中是否至少有一项被选中。
    /// </summary>
    [Description("@#someSelected")]
    public bool SomeSelected { get; init; }

    /// <summary>
    /// 当前可选择条目是否全部被选中。
    /// </summary>
    [Description("@#allSelected")]
    public bool AllSelected { get; init; }

    /// <summary>
    /// 设置当前可选择条目的全选状态。
    /// </summary>
    [Description("@#selectAll")]
    public VDataTableSelectAllCallback? SelectAll { get; init; }

    /// <summary>
    /// 切换指定列的排序方向，并更新当前排序状态。
    /// </summary>
    [Description("@#toggleSort")]
    public VDataTableToggleSortCallback? ToggleSort { get; init; }

    /// <summary>
    /// 检查指定列当前是否参与排序。
    /// </summary>
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
    /// <summary>
    /// 当前单元格对应的列定义。
    /// </summary>
    [Description("@#column")]
    public VuetifyDataTableHeader? Column { get; init; }

    /// <summary>
    /// 当前排序字段和各字段的排序方向，数组顺序表示排序优先级。
    /// </summary>
    [Description("@#sortBy")]
    public VuetifyDataTableSortItem[]? SortBy { get; init; }

    /// <summary>
    /// 当前条目集合中是否至少有一项被选中。
    /// </summary>
    [Description("@#someSelected")]
    public bool SomeSelected { get; init; }

    /// <summary>
    /// 当前可选择条目是否全部被选中。
    /// </summary>
    [Description("@#allSelected")]
    public bool AllSelected { get; init; }

    /// <summary>
    /// 设置当前可选择条目的全选状态。
    /// </summary>
    [Description("@#selectAll")]
    public VDataTableSelectAllCallback? SelectAll { get; init; }

    /// <summary>
    /// 切换指定列的排序方向，并更新当前排序状态。
    /// </summary>
    [Description("@#toggleSort")]
    public VDataTableToggleSortCallback? ToggleSort { get; init; }

    /// <summary>
    /// 检查指定列当前是否参与排序。
    /// </summary>
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
    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VuetifyDataTableItem? Item { get; init; }

    /// <summary>
    /// Vuetify 转换后的内部数据项；包含原始数据及用于渲染、选择的附加信息。
    /// </summary>
    [Description("@#internalItem")]
    public VuetifyDataTableItem? InternalItem { get; init; }

    /// <summary>
    /// 当前可渲染的叶子列定义，顺序与表格显示列一致。
    /// </summary>
    [Description("@#columns")]
    public VuetifyDataTableHeader[]? Columns { get; init; }

    /// <summary>
    /// 检查指定条目是否已选中的回调。
    /// </summary>
    [Description("@#isSelected")]
    public VDataTableIsSelectedCallback? IsSelected { get; init; }

    /// <summary>
    /// 切换指定条目的选中状态。
    /// </summary>
    [Description("@#toggleSelect")]
    public VDataTableToggleSelectCallback? ToggleSelect { get; init; }

    /// <summary>
    /// 检查指定条目的详情行是否已展开。
    /// </summary>
    [Description("@#isExpanded")]
    public VDataTableIsExpandedCallback? IsExpanded { get; init; }

    /// <summary>
    /// 切换指定条目的详情行展开状态。
    /// </summary>
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
    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VuetifyDataTableItem? Item { get; init; }

    /// <summary>
    /// 当前可渲染的叶子列定义，顺序与表格显示列一致。
    /// </summary>
    [Description("@#columns")]
    public VuetifyDataTableHeader[]? Columns { get; init; }

    /// <summary>
    /// 检查指定分组是否已展开。
    /// </summary>
    [Description("@#isGroupOpen")]
    public VDataTableIsGroupOpenCallback? IsGroupOpen { get; init; }

    /// <summary>
    /// 切换指定分组的展开状态。
    /// </summary>
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
    /// <summary>
    /// 当前条目在处理后列表中的索引，从 0 开始。
    /// </summary>
    [Description("@#index")]
    public int Index { get; init; }

    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VuetifyDataTableItem? Item { get; init; }

    /// <summary>
    /// Vuetify 转换后的内部数据项；包含原始数据及用于渲染、选择的附加信息。
    /// </summary>
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
    /// <summary>
    /// 当前条目在处理后列表中的索引，从 0 开始。
    /// </summary>
    [Description("@#index")]
    public int Index { get; init; }

    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VuetifyDataTableItem? Item { get; init; }

    /// <summary>
    /// Vuetify 转换后的内部数据项；包含原始数据及用于渲染、选择的附加信息。
    /// </summary>
    [Description("@#internalItem")]
    public VuetifyDataTableItem? InternalItem { get; init; }

    /// <summary>
    /// 当前单元格对应的列定义。
    /// </summary>
    [Description("@#column")]
    public VuetifyDataTableHeader? Column { get; init; }
}

/// <summary>
/// 用于 VDataTableSlotContext.SetItemsPerPage 的回调签名。
/// 设置每页条目数并更新分页状态。
/// </summary>
public delegate void VDataTableSetItemsPerPageCallback(int itemsPerPage);

/// <summary>
/// 用于 VDataTableSlotContext.ToggleSort、VDataTableHeadersSlotContext.ToggleSort、VDataTableHeaderCellSlotContext.ToggleSort 的回调签名。
/// 切换指定列的排序方向，并更新当前排序状态。
/// </summary>
public delegate void VDataTableToggleSortCallback(VuetifyDataTableHeader header);

/// <summary>
/// 用于 VDataTableSlotContext.IsSorted、VDataTableHeadersSlotContext.IsSorted、VDataTableHeaderCellSlotContext.IsSorted 的回调签名。
/// 检查指定列当前是否参与排序。
/// </summary>
public delegate bool VDataTableIsSortedCallback(VuetifyDataTableHeader header);

/// <summary>
/// 用于 VDataTableHeadersSlotContext.SelectAll、VDataTableHeaderCellSlotContext.SelectAll 的回调签名。
/// 设置当前可选择条目的全选状态。
/// </summary>
public delegate void VDataTableSelectAllCallback(bool selected);

/// <summary>
/// 用于 VDataTableItemSlotContext.IsSelected 的回调签名。
/// 检查指定条目是否已选中的回调。
/// </summary>
public delegate bool VDataTableIsSelectedCallback(VuetifyDataTableItem item);

/// <summary>
/// 用于 VDataTableItemSlotContext.ToggleSelect 的回调签名。
/// 切换指定条目的选中状态。
/// </summary>
public delegate void VDataTableToggleSelectCallback(VuetifyDataTableItem item);

/// <summary>
/// 用于 VDataTableItemSlotContext.IsExpanded 的回调签名。
/// 检查指定条目的详情行是否已展开。
/// </summary>
public delegate bool VDataTableIsExpandedCallback(VuetifyDataTableItem item);

/// <summary>
/// 用于 VDataTableItemSlotContext.ToggleExpand 的回调签名。
/// 切换指定条目的详情行展开状态。
/// </summary>
public delegate void VDataTableToggleExpandCallback(VuetifyDataTableItem item);

/// <summary>
/// 用于 VDataTableGroupHeaderSlotContext.IsGroupOpen 的回调签名。
/// 检查指定分组是否已展开。
/// </summary>
public delegate bool VDataTableIsGroupOpenCallback(VuetifyDataTableItem item);

/// <summary>
/// 用于 VDataTableGroupHeaderSlotContext.ToggleGroup 的回调签名。
/// 切换指定分组的展开状态。
/// </summary>
public delegate void VDataTableToggleGroupCallback(VuetifyDataTableItem item);