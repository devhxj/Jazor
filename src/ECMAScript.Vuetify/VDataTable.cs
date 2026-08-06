using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 数据表格创作代理，用于 RazorVue。
/// Vuetify data table authoring proxy for RazorVue.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDataTable", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
public sealed class VDataTable : ComponentBase
{
    /// <summary>
    /// 选中行的绑定值。
    /// Bound value for selected rows.
    /// </summary>
    [Parameter]
    public VuetifyDataTableSelectedValues? ModelValue { get; set; }

    /// <summary>
    /// 选中行变化时的回调。
    /// Callback when selected rows change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataTableSelectedValues?> ModelValueChanged { get; set; }

    /// <summary>
    /// 表格列头定义。
    /// Column header definitions for the table.
    /// </summary>
    [Parameter]
    public VuetifyDataTableHeaders? Headers { get; set; }

    /// <summary>
    /// 表格数据行。
    /// Data rows for the table.
    /// </summary>
    [Parameter]
    public VuetifyDataTableItems? Items { get; set; }

    /// <summary>
    /// 用于标识行项值的属性键。
    /// Property key used to identify row item values.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemValue { get; set; }

    /// <summary>
    /// 用于判断行是否可选的属性键。
    /// Property key used to determine if a row is selectable.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemSelectable { get; set; }

    /// <summary>
    /// 是否返回完整对象而非键值。
    /// Whether to return the full object instead of the key value.
    /// </summary>
    [Parameter]
    public bool ReturnObject { get; set; }

    /// <summary>
    /// 当前页码。
    /// Current page number.
    /// </summary>
    [Parameter]
    public int Page { get; set; }

    /// <summary>
    /// 页码变化时的回调。
    /// Callback when page number changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    /// <summary>
    /// 每页显示的行数。
    /// Number of items displayed per page.
    /// </summary>
    [Parameter]
    public int ItemsPerPage { get; set; }

    /// <summary>
    /// 每页行数变化时的回调。
    /// Callback when items per page changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> ItemsPerPageChanged { get; set; }

    /// <summary>
    /// 每页行数选项列表。
    /// Options list for items per page selector.
    /// </summary>
    [Parameter]
    public VuetifyDataTableItemsPerPageOptions? ItemsPerPageOptions { get; set; }

    /// <summary>
    /// 当前排序规则。
    /// Current sort criteria.
    /// </summary>
    [Parameter]
    public VuetifyDataTableSortItems? SortBy { get; set; }

    /// <summary>
    /// 排序规则变化时的回调。
    /// Callback when sort criteria change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataTableSortItems?> SortByChanged { get; set; }

    /// <summary>
    /// 当前分组规则。
    /// Current group-by criteria.
    /// </summary>
    [Parameter]
    public VuetifyDataTableSortItems? GroupBy { get; set; }

    /// <summary>
    /// 分组规则变化时的回调。
    /// Callback when group-by criteria change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataTableSortItems?> GroupByChanged { get; set; }

    /// <summary>
    /// 展开行的绑定值。
    /// Bound value for expanded rows.
    /// </summary>
    [Parameter]
    public VuetifyDataTableSelectedValues? Expanded { get; set; }

    /// <summary>
    /// 展开行变化时的回调。
    /// Callback when expanded rows change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataTableSelectedValues?> ExpandedChanged { get; set; }

    /// <summary>
    /// 分页、排序等选项变化时的回调。
    /// Callback when pagination or sort options change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataTableOptions?> OptionsChanged { get; set; }

    /// <summary>
    /// 当前可见行变化时的回调。
    /// Callback when currently visible items change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataTableItems?> CurrentItemsChanged { get; set; }

    /// <summary>
    /// 搜索过滤关键词。
    /// Search filter keyword.
    /// </summary>
    [Parameter]
    public string? Search { get; set; }

    /// <summary>
    /// 是否显示行选择复选框。
    /// Whether to show row selection checkboxes.
    /// </summary>
    [Parameter]
    public bool ShowSelect { get; set; }

    /// <summary>
    /// 行选择策略。
    /// Row selection strategy.
    /// </summary>
    [Parameter]
    public VuetifyDataTableSelectStrategy? SelectStrategy { get; set; }

    /// <summary>
    /// 是否显示行展开图标。
    /// Whether to show row expand icons.
    /// </summary>
    [Parameter]
    public bool ShowExpand { get; set; }

    /// <summary>
    /// 是否点击行时展开。
    /// Whether clicking a row expands it.
    /// </summary>
    [Parameter]
    public bool ExpandOnClick { get; set; }

    /// <summary>
    /// 是否隐藏默认表格主体。
    /// Whether to hide the default table body.
    /// </summary>
    [Parameter]
    public bool HideDefaultBody { get; set; }

    /// <summary>
    /// 是否隐藏默认页脚。
    /// Whether to hide the default footer.
    /// </summary>
    [Parameter]
    public bool HideDefaultFooter { get; set; }

    /// <summary>
    /// 是否隐藏默认表头。
    /// Whether to hide the default header.
    /// </summary>
    [Parameter]
    public bool HideDefaultHeader { get; set; }

    /// <summary>
    /// 是否隐藏无数据提示。
    /// Whether to hide the no-data message.
    /// </summary>
    [Parameter]
    public bool HideNoData { get; set; }

    /// <summary>
    /// 无数据时的提示文本。
    /// Text displayed when there is no data.
    /// </summary>
    [Parameter]
    public string? NoDataText { get; set; }

    /// <summary>
    /// 加载状态或加载文本。
    /// Loading state or loading text.
    /// </summary>
    [Parameter]
    public VuetifyBooleanStringValue? Loading { get; set; }

    /// <summary>
    /// 加载中的提示文本。
    /// Text displayed while loading.
    /// </summary>
    [Parameter]
    public string? LoadingText { get; set; }

    /// <summary>
    /// 是否禁用排序。
    /// Whether to disable sorting.
    /// </summary>
    [Parameter]
    public bool DisableSort { get; set; }

    /// <summary>
    /// 是否允许多列排序。
    /// Whether to allow sorting by multiple columns.
    /// </summary>
    [Parameter]
    public bool MultiSort { get; set; }

    /// <summary>
    /// 是否必须保持至少一列排序。
    /// Whether at least one column must always be sorted.
    /// </summary>
    [Parameter]
    public bool MustSort { get; set; }

    /// <summary>
    /// 升序排序图标。
    /// Icon for ascending sort indicator.
    /// </summary>
    [Parameter]
    public string? SortAscIcon { get; set; }

    /// <summary>
    /// 降序排序图标。
    /// Icon for descending sort indicator.
    /// </summary>
    [Parameter]
    public string? SortDescIcon { get; set; }

    /// <summary>
    /// 组件主题色。
    /// Component theme color.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 组件密度模式。
    /// Component density mode.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否使用紧凑布局（已废弃，使用 Density 代替）。
    /// Whether to use compact layout (deprecated, use Density instead).
    /// </summary>
    [Parameter]
    public bool Dense { get; set; }

    /// <summary>
    /// 是否固定表头。
    /// Whether to fix the table header.
    /// </summary>
    [Parameter]
    public bool FixedHeader { get; set; }

    /// <summary>
    /// 是否固定页脚。
    /// Whether to fix the table footer.
    /// </summary>
    [Parameter]
    public bool FixedFooter { get; set; }

    /// <summary>
    /// 是否在悬停时高亮行。
    /// Whether to highlight rows on hover.
    /// </summary>
    [Parameter]
    public bool Hover { get; set; }

    /// <summary>
    /// 表格高度。
    /// Table height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 表格宽度。
    /// Table width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 行项的唯一标识属性。
    /// Unique identifier property for row items.
    /// </summary>
    [Parameter]
    public string? ItemKey { get; set; }

    /// <summary>
    /// 表头组件属性。
    /// Props for the header component.
    /// </summary>
    [Parameter]
    public VueProps? HeaderProps { get; set; }

    /// <summary>
    /// 行属性配置。
    /// Row props configuration.
    /// </summary>
    [Parameter]
    public VuetifyDataTableRowProps? RowProps { get; set; }

    /// <summary>
    /// 单元格属性配置。
    /// Cell props configuration.
    /// </summary>
    [Parameter]
    public VuetifyDataTableCellProps? CellProps { get; set; }

    /// <summary>
    /// 上一页图标。
    /// Icon for previous page button.
    /// </summary>
    [Parameter]
    public string? PrevIcon { get; set; }

    /// <summary>
    /// 下一页图标。
    /// Icon for next page button.
    /// </summary>
    [Parameter]
    public string? NextIcon { get; set; }

    /// <summary>
    /// 第一页图标。
    /// Icon for first page button.
    /// </summary>
    [Parameter]
    public string? FirstIcon { get; set; }

    /// <summary>
    /// 最后一页图标。
    /// Icon for last page button.
    /// </summary>
    [Parameter]
    public string? LastIcon { get; set; }

    /// <summary>
    /// 每页行数选择器的标签文本。
    /// Label text for the items-per-page selector.
    /// </summary>
    [Parameter]
    public string? ItemsPerPageText { get; set; }

    /// <summary>
    /// 分页信息显示文本。
    /// Pagination info display text.
    /// </summary>
    [Parameter]
    public string? PageText { get; set; }

    /// <summary>
    /// 是否显示当前页码。
    /// Whether to show the current page number.
    /// </summary>
    [Parameter]
    public bool ShowCurrentPage { get; set; }

    /// <summary>
    /// 附加到组件的额外 HTML 属性。
    /// Additional HTML attributes attached to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 表格顶部插槽内容。
    /// Slot content for the top of the table.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Top { get; set; }

    /// <summary>
    /// 列分组插槽内容。
    /// Slot content for the column group element.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Colgroup { get; set; }

    /// <summary>
    /// 表头行插槽内容。
    /// Slot content for the header rows.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableHeadersSlotContext>? HeadersContent { get; set; }

    /// <summary>
    /// 表头选择列插槽内容。
    /// Slot content for the header select column.
    /// </summary>
    [Parameter]
    [ECMAScriptName("header.data-table-select")]
    public RenderFragment<VDataTableHeaderCellSlotContext>? HeaderSelect { get; set; }

    /// <summary>
    /// 表头展开列插槽内容。
    /// Slot content for the header expand column.
    /// </summary>
    [Parameter]
    [ECMAScriptName("header.data-table-expand")]
    public RenderFragment<VDataTableHeaderCellSlotContext>? HeaderExpand { get; set; }

    /// <summary>
    /// 表格主体插槽内容。
    /// Slot content for the table body.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableSlotContext>? BodyContent { get; set; }

    /// <summary>
    /// 表格主体前置插槽内容。
    /// Slot content prepended to the table body.
    /// </summary>
    [Parameter]
    [ECMAScriptName("body.prepend")]
    public RenderFragment<VDataTableSlotContext>? BodyPrepend { get; set; }

    /// <summary>
    /// 表格主体后置插槽内容。
    /// Slot content appended to the table body.
    /// </summary>
    [Parameter]
    [ECMAScriptName("body.append")]
    public RenderFragment<VDataTableSlotContext>? BodyAppend { get; set; }

    /// <summary>
    /// 数据行插槽内容。
    /// Slot content for each data row.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableItemSlotContext>? ItemContent { get; set; }

    /// <summary>
    /// 分组头插槽内容。
    /// Slot content for group headers.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableGroupHeaderSlotContext>? GroupHeader { get; set; }

    /// <summary>
    /// 展开行插槽内容。
    /// Slot content for expanded rows.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableItemSlotContext>? ExpandedRow { get; set; }

    /// <summary>
    /// tbody 元素插槽内容。
    /// Slot content for the tbody element.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Tbody { get; set; }

    /// <summary>
    /// thead 元素插槽内容。
    /// Slot content for the thead element.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Thead { get; set; }

    /// <summary>
    /// tfoot 元素插槽内容。
    /// Slot content for the tfoot element.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Tfoot { get; set; }

    /// <summary>
    /// 表格底部插槽内容。
    /// Slot content for the bottom of the table.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Bottom { get; set; }

    /// <summary>
    /// 页脚前置插槽内容。
    /// Slot content prepended to the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("footer.prepend")]
    public RenderFragment? FooterPrepend { get; set; }

    /// <summary>
    /// 加载状态插槽内容。
    /// Slot content for the loading state.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingContent { get; set; }

    /// <summary>
    /// 无数据插槽内容。
    /// Slot content when there is no data.
    /// </summary>
    [Parameter]
    public RenderFragment? NoData { get; set; }

    /// <summary>
    /// 默认子内容插槽。
    /// Default child content slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
