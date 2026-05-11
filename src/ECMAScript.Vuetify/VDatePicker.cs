using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify date-picker authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDatePicker")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(MonthChanged), VueEmitKind.ModelUpdate, Name = "update:month")]
[VueLibraryEmit(nameof(YearChanged), VueEmitKind.ModelUpdate, Name = "update:year")]
[VueLibraryEmit(nameof(ViewModeChanged), VueEmitKind.ModelUpdate, Name = "update:viewMode")]
[VueLibraryProp(nameof(HeaderText), Name = "header")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
[VueLibrarySlot(nameof(HeaderContent), Name = "header")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
public sealed class VDatePicker : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyDatePickerModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyDatePickerModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyDatePickerMultipleValue? Multiple { get; set; }

    [Parameter]
    public VuetifyDatePickerModelValue? Min { get; set; }

    [Parameter]
    public VuetifyDatePickerModelValue? Max { get; set; }

    [Parameter]
    public int? Year { get; set; }

    [Parameter]
    public EventCallback<int> YearChanged { get; set; }

    [Parameter]
    public VueStringNumberValue? Month { get; set; }

    [Parameter]
    public EventCallback<int> MonthChanged { get; set; }

    [Parameter]
    public VuetifyDatePickerViewMode? ViewMode { get; set; }

    [Parameter]
    public EventCallback<VuetifyDatePickerViewMode> ViewModeChanged { get; set; }

    [Parameter]
    public VuetifyDatePickerActiveValue? Active { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ShowAdjacentMonths { get; set; }

    [Parameter]
    public VuetifyCalendarWeekdays? Weekdays { get; set; }

    [Parameter]
    public VuetifyDatePickerWeeksInMonth? WeeksInMonth { get; set; }

    [Parameter]
    public VueStringNumberValue? FirstDayOfWeek { get; set; }

    [Parameter]
    public VuetifyDatePickerAllowedDatesValue? AllowedDates { get; set; }

    [Parameter]
    public bool HideWeekdays { get; set; }

    [Parameter]
    public bool ShowWeek { get; set; }

    [Parameter]
    public string? Transition { get; set; }

    [Parameter]
    public string? ReverseTransition { get; set; }

    [Parameter]
    public VueStringNumberValue? ControlHeight { get; set; }

    [Parameter]
    public VuetifyIconValue? NextIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? PrevIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? ModeIcon { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? HeaderText { get; set; }

    [Parameter]
    public string? HeaderColor { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public VuetifyPosition? Position { get; set; }

    [Parameter]
    public VuetifyLocation? Location { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public bool Divided { get; set; }

    [Parameter]
    public bool Landscape { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public bool HideHeader { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment<VDatePickerHeaderSlotContext>? HeaderContent { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }
}
