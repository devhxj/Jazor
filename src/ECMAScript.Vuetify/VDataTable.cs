using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 数据表格创作代理，用于 RazorVue。
/// Vuetify data table authoring proxy for RazorVue.
/// </summary>
[ECMAScript("vuetify/components/VDataTable")]
public sealed class VDataTable : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 选中行的绑定值。
    /// Bound value for selected rows.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDataTableSelectedValues?；值域为 VueValue[]。变量用 ModelValue="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyDataTableSelectedValues? ModelValue { get; set; }

    /// <summary>
    /// 选中行变化时的回调。
    /// Callback when selected rows change.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyDataTableSelectedValues?；保持与模型相同的强类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyDataTableSelectedValues?> ModelValueChanged { get; set; }

    /// <summary>
    /// 表格列头定义。
    /// Column header definitions for the table.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDataTableHeaders?；值域为 VuetifyDataTableHeader[]。变量用 Headers="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("headers")]
    public VuetifyDataTableHeaders? Headers { get; set; }

    /// <summary>
    /// 表格数据行。
    /// Data rows for the table.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDataTableItems?；值域为 VuetifyDataTableItem[]。变量用 Items="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("items")]
    public VuetifyDataTableItems? Items { get; set; }

    /// <summary>
    /// 用于标识行项值的属性键。
    /// Property key used to identify row item values.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySelectItemKey?；值域为 string | string[] | VuetifySelectItemKeySelector | bool。变量用 ItemValue="@value"，并保持声明的分支类型。字符串用 ItemValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsPath、value?.AsSelector、value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("itemValue")]
    public VuetifySelectItemKey? ItemValue { get; set; }

    /// <summary>
    /// 用于判断行是否可选的属性键。
    /// Property key used to determine if a row is selectable.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySelectItemKey?；值域为 string | string[] | VuetifySelectItemKeySelector | bool。变量用 ItemSelectable="@value"，并保持声明的分支类型。字符串用 ItemSelectable="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsPath、value?.AsSelector、value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("itemSelectable")]
    public VuetifySelectItemKey? ItemSelectable { get; set; }

    /// <summary>
    /// 是否返回完整对象而非键值。
    /// Whether to return the full object instead of the key value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("returnObject")]
    public bool ReturnObject { get; set; }

    /// <summary>
    /// 当前页码。
    /// Current page number.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 int；Razor 数值写 Page="@(32)" 或 Page="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("page")]
    public int Page { get; set; }

    /// <summary>
    /// 页码变化时的回调。
    /// Callback when page number changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 int；保持与模型相同的强类型。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:page")]
    public EventCallback<int> PageChanged { get; set; }

    /// <summary>
    /// 每页显示的行数。
    /// Number of items displayed per page.
    /// </summary>
    /// <remarks data-authoring="types">C# 类型为 int；Razor 数值写 ItemsPerPage="@(32)" 或 ItemsPerPage="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
    [Parameter]
    [ECMAScriptName("itemsPerPage")]
    public int ItemsPerPage { get; set; }

    /// <summary>
    /// 每页行数变化时的回调。
    /// Callback when items per page changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 int；保持与模型相同的强类型。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:itemsPerPage")]
    public EventCallback<int> ItemsPerPageChanged { get; set; }

    /// <summary>
    /// 每页行数选项列表。
    /// Options list for items per page selector.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDataTableItemsPerPageOptions?；值域为 VuetifyDataTableItemsPerPageOption[]。变量用 ItemsPerPageOptions="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("itemsPerPageOptions")]
    public VuetifyDataTableItemsPerPageOptions? ItemsPerPageOptions { get; set; }

    /// <summary>
    /// 当前排序规则。
    /// Current sort criteria.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDataTableSortItems?；值域为 VuetifyDataTableSortItem[]。变量用 SortBy="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("sortBy")]
    public VuetifyDataTableSortItems? SortBy { get; set; }

    /// <summary>
    /// 排序规则变化时的回调。
    /// Callback when sort criteria change.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyDataTableSortItems?；保持与模型相同的强类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:sortBy")]
    public EventCallback<VuetifyDataTableSortItems?> SortByChanged { get; set; }

    /// <summary>
    /// 当前分组规则。
    /// Current group-by criteria.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDataTableSortItems?；值域为 VuetifyDataTableSortItem[]。变量用 GroupBy="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("groupBy")]
    public VuetifyDataTableSortItems? GroupBy { get; set; }

    /// <summary>
    /// 分组规则变化时的回调。
    /// Callback when group-by criteria change.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyDataTableSortItems?；保持与模型相同的强类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:groupBy")]
    public EventCallback<VuetifyDataTableSortItems?> GroupByChanged { get; set; }

    /// <summary>
    /// 展开行的绑定值。
    /// Bound value for expanded rows.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDataTableSelectedValues?；值域为 VueValue[]。变量用 Expanded="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("expanded")]
    public VuetifyDataTableSelectedValues? Expanded { get; set; }

    /// <summary>
    /// 展开行变化时的回调。
    /// Callback when expanded rows change.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyDataTableSelectedValues?；保持与模型相同的强类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:expanded")]
    public EventCallback<VuetifyDataTableSelectedValues?> ExpandedChanged { get; set; }

    /// <summary>
    /// 分页、排序等选项变化时的回调。
    /// Callback when pagination or sort options change.
    /// </summary>
    [Parameter]
    [ECMAScriptName("optionsChanged")]
    public EventCallback<VuetifyDataTableOptions?> OptionsChanged { get; set; }

    /// <summary>
    /// 当前可见行变化时的回调。
    /// Callback when currently visible items change.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyDataTableItems?；保持与模型相同的强类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("currentItemsChanged")]
    public EventCallback<VuetifyDataTableItems?> CurrentItemsChanged { get; set; }

    /// <summary>
    /// 搜索过滤关键词。
    /// Search filter keyword.
    /// </summary>
    [Parameter]
    [ECMAScriptName("search")]
    public string? Search { get; set; }

    /// <summary>
    /// 是否显示行选择复选框。
    /// Whether to show row selection checkboxes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showSelect")]
    public bool ShowSelect { get; set; }

    /// <summary>
    /// 行选择策略。
    /// Row selection strategy.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectStrategy")]
    public VuetifyDataTableSelectStrategy? SelectStrategy { get; set; }

    /// <summary>
    /// 是否显示行展开图标。
    /// Whether to show row expand icons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showExpand")]
    public bool ShowExpand { get; set; }

    /// <summary>
    /// 是否点击行时展开。
    /// Whether clicking a row expands it.
    /// </summary>
    [Parameter]
    [ECMAScriptName("expandOnClick")]
    public bool ExpandOnClick { get; set; }

    /// <summary>
    /// 是否隐藏默认表格主体。
    /// Whether to hide the default table body.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideDefaultBody")]
    public bool HideDefaultBody { get; set; }

    /// <summary>
    /// 是否隐藏默认页脚。
    /// Whether to hide the default footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideDefaultFooter")]
    public bool HideDefaultFooter { get; set; }

    /// <summary>
    /// 是否隐藏默认表头。
    /// Whether to hide the default header.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideDefaultHeader")]
    public bool HideDefaultHeader { get; set; }

    /// <summary>
    /// 是否隐藏无数据提示。
    /// Whether to hide the no-data message.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideNoData")]
    public bool HideNoData { get; set; }

    /// <summary>
    /// 无数据时的提示文本。
    /// Text displayed when there is no data.
    /// </summary>
    [Parameter]
    [ECMAScriptName("noDataText")]
    public string? NoDataText { get; set; }

    /// <summary>
    /// 加载状态或加载文本。
    /// Loading state or loading text.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBooleanStringValue?；值域为 bool | string。变量用 Loading="@value"，并保持声明的分支类型。字符串用 Loading="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("loading")]
    public VuetifyBooleanStringValue? Loading { get; set; }

    /// <summary>
    /// 加载中的提示文本。
    /// Text displayed while loading.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loadingText")]
    public string? LoadingText { get; set; }

    /// <summary>
    /// 是否禁用排序。
    /// Whether to disable sorting.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disableSort")]
    public bool DisableSort { get; set; }

    /// <summary>
    /// 是否允许多列排序。
    /// Whether to allow sorting by multiple columns.
    /// </summary>
    [Parameter]
    [ECMAScriptName("multiSort")]
    public bool MultiSort { get; set; }

    /// <summary>
    /// 是否必须保持至少一列排序。
    /// Whether at least one column must always be sorted.
    /// </summary>
    [Parameter]
    [ECMAScriptName("mustSort")]
    public bool MustSort { get; set; }

    /// <summary>
    /// 升序排序图标。
    /// Icon for ascending sort indicator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("sortAscIcon")]
    public string? SortAscIcon { get; set; }

    /// <summary>
    /// 降序排序图标。
    /// Icon for descending sort indicator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("sortDescIcon")]
    public string? SortDescIcon { get; set; }

    /// <summary>
    /// 组件主题色。
    /// Component theme color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 组件密度模式。
    /// Component density mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否使用紧凑布局（已废弃，使用 Density 代替）。
    /// Whether to use compact layout (deprecated, use Density instead).
    /// </summary>
    [Parameter]
    [ECMAScriptName("dense")]
    public bool Dense { get; set; }

    /// <summary>
    /// 是否固定表头。
    /// Whether to fix the table header.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fixedHeader")]
    public bool FixedHeader { get; set; }

    /// <summary>
    /// 是否固定页脚。
    /// Whether to fix the table footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fixedFooter")]
    public bool FixedFooter { get; set; }

    /// <summary>
    /// 是否在悬停时高亮行。
    /// Whether to highlight rows on hover.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hover")]
    public bool Hover { get; set; }

    /// <summary>
    /// 表格高度。
    /// Table height.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Height="@(32)"；变量用 Height="@value"，无需 double 后缀。字符串用 Height="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 表格宽度。
    /// Table width.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Width="@(32)"；变量用 Width="@value"，无需 double 后缀。字符串用 Width="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 行项的唯一标识属性。
    /// Unique identifier property for row items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemKey")]
    public string? ItemKey { get; set; }

    /// <summary>
    /// 表头组件属性。
    /// Props for the header component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("headerProps")]
    public VueProps? HeaderProps { get; set; }

    /// <summary>
    /// 行属性配置。
    /// Row props configuration.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDataTableRowProps?；值域为 VueProps | VuetifyDataTableRowPropsCallback。变量用 RowProps="@value"，并保持声明的分支类型。投影：value?.AsProps、value?.AsCallback；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("rowProps")]
    public VuetifyDataTableRowProps? RowProps { get; set; }

    /// <summary>
    /// 单元格属性配置。
    /// Cell props configuration.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDataTableCellProps?；值域为 VueProps | VuetifyDataTableCellPropsCallback。变量用 CellProps="@value"，并保持声明的分支类型。投影：value?.AsProps、value?.AsCallback；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("cellProps")]
    public VuetifyDataTableCellProps? CellProps { get; set; }

    /// <summary>
    /// 上一页图标。
    /// Icon for previous page button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prevIcon")]
    public string? PrevIcon { get; set; }

    /// <summary>
    /// 下一页图标。
    /// Icon for next page button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("nextIcon")]
    public string? NextIcon { get; set; }

    /// <summary>
    /// 第一页图标。
    /// Icon for first page button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("firstIcon")]
    public string? FirstIcon { get; set; }

    /// <summary>
    /// 最后一页图标。
    /// Icon for last page button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("lastIcon")]
    public string? LastIcon { get; set; }

    /// <summary>
    /// 每页行数选择器的标签文本。
    /// Label text for the items-per-page selector.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemsPerPageText")]
    public string? ItemsPerPageText { get; set; }

    /// <summary>
    /// 分页信息显示文本。
    /// Pagination info display text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("pageText")]
    public string? PageText { get; set; }

    /// <summary>
    /// 是否显示当前页码。
    /// Whether to show the current page number.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showCurrentPage")]
    public bool ShowCurrentPage { get; set; }

    /// <summary>
    /// 附加到组件的额外 HTML 属性。
    /// Additional HTML attributes attached to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 表格顶部插槽内容。
    /// Slot content for the top of the table.
    /// </summary>
    [Parameter]
    [ECMAScriptName("top")]
    public RenderFragment<VDataTableSlotContext>? Top { get; set; }

    /// <summary>
    /// 列分组插槽内容。
    /// Slot content for the column group element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("colgroup")]
    public RenderFragment<VDataTableSlotContext>? Colgroup { get; set; }

    /// <summary>
    /// 表头行插槽内容。
    /// Slot content for the header rows.
    /// </summary>
    [Parameter]
    [ECMAScriptName("headers")]
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
    [ECMAScriptName("body")]
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
    [ECMAScriptName("item")]
    public RenderFragment<VDataTableItemSlotContext>? ItemContent { get; set; }

    /// <summary>
    /// 分组头插槽内容。
    /// Slot content for group headers.
    /// </summary>
    [Parameter]
    [ECMAScriptName("group-header")]
    public RenderFragment<VDataTableGroupHeaderSlotContext>? GroupHeader { get; set; }

    /// <summary>
    /// 展开行插槽内容。
    /// Slot content for expanded rows.
    /// </summary>
    [Parameter]
    [ECMAScriptName("expanded-row")]
    public RenderFragment<VDataTableItemSlotContext>? ExpandedRow { get; set; }

    /// <summary>
    /// tbody 元素插槽内容。
    /// Slot content for the tbody element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tbody")]
    public RenderFragment<VDataTableSlotContext>? Tbody { get; set; }

    /// <summary>
    /// thead 元素插槽内容。
    /// Slot content for the thead element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("thead")]
    public RenderFragment<VDataTableSlotContext>? Thead { get; set; }

    /// <summary>
    /// tfoot 元素插槽内容。
    /// Slot content for the tfoot element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tfoot")]
    public RenderFragment<VDataTableSlotContext>? Tfoot { get; set; }

    /// <summary>
    /// 表格底部插槽内容。
    /// Slot content for the bottom of the table.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bottom")]
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
    [ECMAScriptName("loading")]
    public RenderFragment? LoadingContent { get; set; }

    /// <summary>
    /// 无数据插槽内容。
    /// Slot content when there is no data.
    /// </summary>
    [Parameter]
    [ECMAScriptName("no-data")]
    public RenderFragment? NoData { get; set; }

    /// <summary>
    /// 默认子内容插槽。
    /// Default child content slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
