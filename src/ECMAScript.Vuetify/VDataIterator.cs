using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify data-iterator authoring proxy for filtered, sorted, grouped, paginated item rendering.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDataIterator")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(OptionsChanged), VueEmitKind.ModelUpdate, Name = "update:options")]
[VueLibraryEmit(nameof(CurrentItemsChanged), VueEmitKind.ModelUpdate, Name = "update:currentItems")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Header), Name = "header")]
[VueLibrarySlot(nameof(Footer), Name = "footer")]
[VueLibrarySlot(nameof(Loader), Name = "loader")]
[VueLibrarySlot(nameof(NoData), Name = "no-data")]
public sealed class VDataIterator : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyDataIteratorSelectedValues? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataIteratorSelectedValues?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyDataIteratorItems? Items { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemValue { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemSelectable { get; set; }

    [Parameter]
    public bool ReturnObject { get; set; }

    [Parameter]
    public VueStringNumberValue? Page { get; set; }

    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    [Parameter]
    public VueStringNumberValue? ItemsPerPage { get; set; }

    [Parameter]
    public EventCallback<int> ItemsPerPageChanged { get; set; }

    [Parameter]
    public VuetifyDataTableSortItems? SortBy { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataTableSortItems?> SortByChanged { get; set; }

    [Parameter]
    public VuetifyDataTableSortItems? GroupBy { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataTableSortItems?> GroupByChanged { get; set; }

    [Parameter]
    public string[]? Expanded { get; set; }

    [Parameter]
    public EventCallback<string[]?> ExpandedChanged { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataTableOptions?> OptionsChanged { get; set; }

    [Parameter]
    public EventCallback<VuetifyDataIteratorItems?> CurrentItemsChanged { get; set; }

    [Parameter]
    public string? Search { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public bool ShowSelect { get; set; }

    [Parameter]
    public VuetifyDataTableSelectStrategy? SelectStrategy { get; set; }

    [Parameter]
    public VuetifyDataIteratorValueComparator? ValueComparator { get; set; }

    [Parameter]
    public bool ShowExpand { get; set; }

    [Parameter]
    public bool ExpandOnClick { get; set; }

    [Parameter]
    public bool MultiSort { get; set; }

    [Parameter]
    public bool MustSort { get; set; }

    [Parameter]
    public VuetifyDataIteratorSortFunctions? CustomKeySort { get; set; }

    [Parameter]
    public VuetifyFilterFunction? CustomFilter { get; set; }

    [Parameter]
    public VuetifyFilterKeyFunctions? CustomKeyFilter { get; set; }

    [Parameter]
    public VuetifyFilterKeys? FilterKeys { get; set; }

    [Parameter]
    public VuetifyFilterMode? FilterMode { get; set; }

    [Parameter]
    public bool NoFilter { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VDataIteratorSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VDataIteratorSlotContext>? Header { get; set; }

    [Parameter]
    public RenderFragment<VDataIteratorSlotContext>? Footer { get; set; }

    [Parameter]
    public RenderFragment<VuetifyLoaderSlotContext>? Loader { get; set; }

    [Parameter]
    public RenderFragment? NoData { get; set; }
}
