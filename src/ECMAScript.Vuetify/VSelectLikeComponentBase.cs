using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Shared Vuetify select-family authoring surface.
/// </summary>
public abstract class VSelectLikeComponentBase : ComponentBase
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? Hint { get; set; }

    [Parameter]
    public bool PersistentHint { get; set; }

    [Parameter]
    public bool PersistentPlaceholder { get; set; }

    [Parameter]
    public bool PersistentClear { get; set; }

    [Parameter]
    public string? Prefix { get; set; }

    [Parameter]
    public string? Suffix { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public bool Focused { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public VuetifyMessagesValue? ErrorMessages { get; set; }

    [Parameter]
    public VuetifyMessagesValue? Messages { get; set; }

    [Parameter]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    [Parameter]
    public VuetifyValidateOn? ValidateOn { get; set; }

    [Parameter]
    public string? PrependIcon { get; set; }

    [Parameter]
    public string? AppendIcon { get; set; }

    [Parameter]
    public string? PrependInnerIcon { get; set; }

    [Parameter]
    public string? ClearIcon { get; set; }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public VuetifyFieldVariant? Variant { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VuetifySelectItems? Items { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemTitle { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemValue { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemChildren { get; set; }

    [Parameter]
    public VuetifySelectItemPropsSelector? ItemProps { get; set; }

    [Parameter]
    public VuetifySelectValueComparator? ValueComparator { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public bool ReturnObject { get; set; }

    [Parameter]
    public bool Chips { get; set; }

    [Parameter]
    public bool ClosableChips { get; set; }

    [Parameter]
    public bool Eager { get; set; }

    [Parameter]
    public bool HideNoData { get; set; }

    [Parameter]
    public bool HideSelected { get; set; }

    [Parameter]
    public VueProps? ListProps { get; set; }

    [Parameter]
    public bool Menu { get; set; }

    [Parameter]
    public EventCallback<bool> MenuChanged { get; set; }

    [Parameter]
    public string? MenuIcon { get; set; }

    [Parameter]
    public VueProps? MenuProps { get; set; }

    [Parameter]
    public string? NoDataText { get; set; }

    [Parameter]
    public bool OpenOnClear { get; set; }

    [Parameter]
    public string? CloseText { get; set; }

    [Parameter]
    public string? OpenText { get; set; }

    [Parameter]
    public string? ItemColor { get; set; }

    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

    [Parameter]
    public RenderFragment<VSelectItemSlotContext>? Item { get; set; }

    [Parameter]
    public RenderFragment<VSelectChipSlotContext>? Chip { get; set; }

    [Parameter]
    public RenderFragment<VSelectSelectionSlotContext>? Selection { get; set; }

    [Parameter]
    public RenderFragment? PrependItem { get; set; }

    [Parameter]
    public RenderFragment? AppendItem { get; set; }

    [Parameter]
    public RenderFragment? NoData { get; set; }
}
