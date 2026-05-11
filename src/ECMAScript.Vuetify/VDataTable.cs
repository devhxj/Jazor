using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify data table authoring proxy for RazorVue.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDataTable")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(PageChanged), VueEmitKind.ModelUpdate, Name = "update:page")]
[VueLibraryEmit(nameof(ItemsPerPageChanged), VueEmitKind.ModelUpdate, Name = "update:itemsPerPage")]
[VueLibraryEmit(nameof(SortByChanged), VueEmitKind.ModelUpdate, Name = "update:sortBy")]
[VueLibraryEmit(nameof(GroupByChanged), VueEmitKind.ModelUpdate, Name = "update:groupBy")]
[VueLibraryEmit(nameof(ExpandedChanged), VueEmitKind.ModelUpdate, Name = "update:expanded")]
[VueLibraryEmit(nameof(OptionsChanged), VueEmitKind.ModelUpdate, Name = "update:options")]
[VueLibraryEmit(nameof(CurrentItemsChanged), VueEmitKind.ModelUpdate, Name = "update:currentItems")]
[VueLibrarySlot(nameof(Top), Name = "top")]
[VueLibrarySlot(nameof(Colgroup), Name = "colgroup")]
[VueLibrarySlot(nameof(HeadersContent), Name = "headers")]
[VueLibrarySlot(nameof(HeaderSelect), Name = "header.data-table-select")]
[VueLibrarySlot(nameof(HeaderExpand), Name = "header.data-table-expand")]
[VueLibrarySlot(nameof(BodyContent), Name = "body")]
[VueLibrarySlot(nameof(BodyPrepend), Name = "body.prepend")]
[VueLibrarySlot(nameof(BodyAppend), Name = "body.append")]
[VueLibrarySlot(nameof(ItemContent), Name = "item")]
[VueLibrarySlot(nameof(GroupHeader), Name = "group-header")]
[VueLibrarySlot(nameof(ExpandedRow), Name = "expanded-row")]
[VueLibrarySlot(nameof(Tbody), Name = "tbody")]
[VueLibrarySlot(nameof(Thead), Name = "thead")]
[VueLibrarySlot(nameof(Tfoot), Name = "tfoot")]
[VueLibrarySlot(nameof(Bottom), Name = "bottom")]
[VueLibrarySlot(nameof(FooterPrepend), Name = "footer.prepend")]
[VueLibrarySlot(nameof(LoadingContent), Name = "loading")]
[VueLibrarySlot(nameof(NoData), Name = "no-data")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VDataTable : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyDataTableSelectedValues? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataTableSelectedValues?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyDataTableHeaders? Headers { get; set; }

    [Parameter]
    public VuetifyDataTableItems? Items { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemValue { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemSelectable { get; set; }

    [Parameter]
    public bool ReturnObject { get; set; }

    [Parameter]
    public int Page { get; set; }

    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    [Parameter]
    public int ItemsPerPage { get; set; }

    [Parameter]
    public EventCallback<int> ItemsPerPageChanged { get; set; }

    [Parameter]
    public VuetifyDataTableItemsPerPageOptions? ItemsPerPageOptions { get; set; }

    [Parameter]
    public VuetifyDataTableSortItems? SortBy { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataTableSortItems?> SortByChanged { get; set; }

    [Parameter]
    public VuetifyDataTableSortItems? GroupBy { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataTableSortItems?> GroupByChanged { get; set; }

    [Parameter]
    public VuetifyDataTableSelectedValues? Expanded { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataTableSelectedValues?> ExpandedChanged { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataTableOptions?> OptionsChanged { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataTableItems?> CurrentItemsChanged { get; set; }

    [Parameter]
    public string? Search { get; set; }

    [Parameter]
    public bool ShowSelect { get; set; }

    [Parameter]
    public VuetifyDataTableSelectStrategy? SelectStrategy { get; set; }

    [Parameter]
    public bool ShowExpand { get; set; }

    [Parameter]
    public bool ExpandOnClick { get; set; }

    [Parameter]
    public bool HideDefaultBody { get; set; }

    [Parameter]
    public bool HideDefaultFooter { get; set; }

    [Parameter]
    public bool HideDefaultHeader { get; set; }

    [Parameter]
    public bool HideNoData { get; set; }

    [Parameter]
    public string? NoDataText { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Loading { get; set; }

    [Parameter]
    public string? LoadingText { get; set; }

    [Parameter]
    public bool DisableSort { get; set; }

    [Parameter]
    public bool MultiSort { get; set; }

    [Parameter]
    public bool MustSort { get; set; }

    [Parameter]
    public string? SortAscIcon { get; set; }

    [Parameter]
    public string? SortDescIcon { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public bool FixedHeader { get; set; }

    [Parameter]
    public bool FixedFooter { get; set; }

    [Parameter]
    public bool Hover { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public string? ItemKey { get; set; }

    [Parameter]
    public VueProps? HeaderProps { get; set; }

    [Parameter]
    public VuetifyDataTableRowProps? RowProps { get; set; }

    [Parameter]
    public VuetifyDataTableCellProps? CellProps { get; set; }

    [Parameter]
    public string? PrevIcon { get; set; }

    [Parameter]
    public string? NextIcon { get; set; }

    [Parameter]
    public string? FirstIcon { get; set; }

    [Parameter]
    public string? LastIcon { get; set; }

    [Parameter]
    public string? ItemsPerPageText { get; set; }

    [Parameter]
    public string? PageText { get; set; }

    [Parameter]
    public bool ShowCurrentPage { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Top { get; set; }

    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Colgroup { get; set; }

    [Parameter]
    public RenderFragment<VDataTableHeadersSlotContext>? HeadersContent { get; set; }

    [Parameter]
    public RenderFragment<VDataTableHeaderCellSlotContext>? HeaderSelect { get; set; }

    [Parameter]
    public RenderFragment<VDataTableHeaderCellSlotContext>? HeaderExpand { get; set; }

    [Parameter]
    public RenderFragment<VDataTableSlotContext>? BodyContent { get; set; }

    [Parameter]
    public RenderFragment<VDataTableSlotContext>? BodyPrepend { get; set; }

    [Parameter]
    public RenderFragment<VDataTableSlotContext>? BodyAppend { get; set; }

    [Parameter]
    public RenderFragment<VDataTableItemSlotContext>? ItemContent { get; set; }

    [Parameter]
    public RenderFragment<VDataTableGroupHeaderSlotContext>? GroupHeader { get; set; }

    [Parameter]
    public RenderFragment<VDataTableItemSlotContext>? ExpandedRow { get; set; }

    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Tbody { get; set; }

    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Thead { get; set; }

    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Tfoot { get; set; }

    [Parameter]
    public RenderFragment<VDataTableSlotContext>? Bottom { get; set; }

    [Parameter]
    public RenderFragment? FooterPrepend { get; set; }

    [Parameter]
    public RenderFragment? LoadingContent { get; set; }

    [Parameter]
    public RenderFragment? NoData { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
