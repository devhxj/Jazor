using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 数据迭代器创作代理，用于过滤、排序、分组和分页的项渲染。
/// Vuetify data-iterator authoring proxy for filtered, sorted, grouped, paginated item rendering.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDataIterator", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
public sealed class VDataIterator : ComponentBase
{
    /// <summary>
    /// 选中项的绑定值。
    /// The bound value of the selected items.
    /// </summary>
    [Parameter]
    public VuetifyDataIteratorSelectedValues? ModelValue { get; set; }

    /// <summary>
    /// 选中项变更回调。
    /// Callback invoked when the selected items change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataIteratorSelectedValues?> ModelValueChanged { get; set; }

    /// <summary>
    /// 数据项集合。
    /// The collection of data items.
    /// </summary>
    [Parameter]
    public VuetifyDataIteratorItems? Items { get; set; }

    /// <summary>
    /// 用于标识项值的字段键。
    /// The key used to identify item values.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemValue { get; set; }

    /// <summary>
    /// 用于标识可选项的字段键。
    /// The key used to identify selectable items.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemSelectable { get; set; }

    /// <summary>
    /// 是否返回完整对象而非键值。
    /// Whether to return full objects instead of key values.
    /// </summary>
    [Parameter]
    public bool ReturnObject { get; set; }

    /// <summary>
    /// 当前页码。
    /// The current page number.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Page { get; set; }

    /// <summary>
    /// 页码变更回调。
    /// Callback invoked when the page changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    /// <summary>
    /// 每页显示项数。
    /// The number of items displayed per page.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? ItemsPerPage { get; set; }

    /// <summary>
    /// 每页项数变更回调。
    /// Callback invoked when the items-per-page count changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> ItemsPerPageChanged { get; set; }

    /// <summary>
    /// 排序规则。
    /// The sort criteria.
    /// </summary>
    [Parameter]
    public VuetifyDataTableSortItems? SortBy { get; set; }

    /// <summary>
    /// 排序规则变更回调。
    /// Callback invoked when the sort criteria change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataTableSortItems?> SortByChanged { get; set; }

    /// <summary>
    /// 分组规则。
    /// The group-by criteria.
    /// </summary>
    [Parameter]
    public VuetifyDataTableSortItems? GroupBy { get; set; }

    /// <summary>
    /// 分组规则变更回调。
    /// Callback invoked when the group-by criteria change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataTableSortItems?> GroupByChanged { get; set; }

    /// <summary>
    /// 展开行的标识数组。
    /// The array of identifiers for expanded rows.
    /// </summary>
    [Parameter]
    public string[]? Expanded { get; set; }

    /// <summary>
    /// 展开行变更回调。
    /// Callback invoked when expanded rows change.
    /// </summary>
    [Parameter]
    public EventCallback<string[]?> ExpandedChanged { get; set; }

    /// <summary>
    /// 分页/排序/分组选项变更回调。
    /// Callback invoked when pagination/sort/group options change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataTableOptions?> OptionsChanged { get; set; }

    /// <summary>
    /// 当前显示项变更回调。
    /// Callback invoked when the currently displayed items change.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyDataIteratorItems?> CurrentItemsChanged { get; set; }

    /// <summary>
    /// 搜索过滤文本。
    /// The search filter text.
    /// </summary>
    [Parameter]
    public string? Search { get; set; }

    /// <summary>
    /// 是否显示加载状态。
    /// Whether to show the loading state.
    /// </summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>
    /// 是否显示行选择框。
    /// Whether to show row selection checkboxes.
    /// </summary>
    [Parameter]
    public bool ShowSelect { get; set; }

    /// <summary>
    /// 行选择策略。
    /// The row selection strategy.
    /// </summary>
    [Parameter]
    public VuetifyDataTableSelectStrategy? SelectStrategy { get; set; }

    /// <summary>
    /// 值比较函数。
    /// The value comparator function.
    /// </summary>
    [Parameter]
    public VuetifyDataIteratorValueComparator? ValueComparator { get; set; }

    /// <summary>
    /// 是否显示展开切换。
    /// Whether to show the expand toggle.
    /// </summary>
    [Parameter]
    public bool ShowExpand { get; set; }

    /// <summary>
    /// 是否在点击时展开行。
    /// Whether to expand rows on click.
    /// </summary>
    [Parameter]
    public bool ExpandOnClick { get; set; }

    /// <summary>
    /// 是否启用多列排序。
    /// Whether to enable multi-column sorting.
    /// </summary>
    [Parameter]
    public bool MultiSort { get; set; }

    /// <summary>
    /// 是否强制排序。
    /// Whether to force sorting.
    /// </summary>
    [Parameter]
    public bool MustSort { get; set; }

    /// <summary>
    /// 自定义按键排序函数集合。
    /// The custom key-sort functions.
    /// </summary>
    [Parameter]
    public VuetifyDataIteratorSortFunctions? CustomKeySort { get; set; }

    /// <summary>
    /// 自定义筛选函数。
    /// The custom filter function.
    /// </summary>
    [Parameter]
    public VuetifyFilterFunction? CustomFilter { get; set; }

    /// <summary>
    /// 自定义按键筛选函数集合。
    /// The custom key-filter functions.
    /// </summary>
    [Parameter]
    public VuetifyFilterKeyFunctions? CustomKeyFilter { get; set; }

    /// <summary>
    /// 用于筛选的字段键。
    /// The keys used for filtering items.
    /// </summary>
    [Parameter]
    public VuetifyFilterKeys? FilterKeys { get; set; }

    /// <summary>
    /// 筛选匹配模式。
    /// The filter matching mode.
    /// </summary>
    [Parameter]
    public VuetifyFilterMode? FilterMode { get; set; }

    /// <summary>
    /// 是否禁用筛选。
    /// Whether to disable filtering.
    /// </summary>
    [Parameter]
    public bool NoFilter { get; set; }

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 过渡动画效果。
    /// The transition animation effect.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Default slot for child content.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataIteratorSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 头部插槽。
    /// Slot for the header area.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataIteratorSlotContext>? Header { get; set; }

    /// <summary>
    /// 底部插槽。
    /// Slot for the footer area.
    /// </summary>
    [Parameter]
    public RenderFragment<VDataIteratorSlotContext>? Footer { get; set; }

    /// <summary>
    /// 加载中插槽。
    /// Slot for the loading state.
    /// </summary>
    [Parameter]
    public RenderFragment<VuetifyLoaderSlotContext>? Loader { get; set; }

    /// <summary>
    /// 无数据插槽。
    /// Slot for the no-data state.
    /// </summary>
    [Parameter]
    public RenderFragment? NoData { get; set; }
}
