using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify Labs 日历创作代理。
/// Vuetify labs calendar authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VCalendar")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(Next), VueEmitKind.LibrarySpecific, Name = "next")]
[VueLibraryEmit(nameof(Prev), VueEmitKind.LibrarySpecific, Name = "prev")]
[VueLibrarySlot(nameof(Header), Name = "header")]
[VueLibrarySlot(nameof(EventContent), Name = "event")]
public sealed class VCalendar : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyCalendarDateValues? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyCalendarDateValues?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyIconValue? NextIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? PrevIcon { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public VuetifyCalendarViewMode? ViewMode { get; set; }

    [Parameter]
    public VuetifyCalendarDay? Day { get; set; }

    [Parameter]
    public Number? DayIndex { get; set; }

    [Parameter]
    public VuetifyCalendarEvents? Events { get; set; }

    [Parameter]
    public Number? IntervalDivisions { get; set; }

    [Parameter]
    public Number? IntervalDuration { get; set; }

    [Parameter]
    public Number? IntervalHeight { get; set; }

    [Parameter]
    public VuetifyCalendarIntervalFormatValue? IntervalFormat { get; set; }

    [Parameter]
    public Number? IntervalStart { get; set; }

    [Parameter]
    public bool HideDayHeader { get; set; }

    [Parameter]
    public Number? Intervals { get; set; }

    [Parameter]
    public VuetifyCalendarAllowedDatesValue? AllowedDates { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VuetifyCalendarDateValue? DisplayValue { get; set; }

    [Parameter]
    public VueStringNumberValue? Month { get; set; }

    [Parameter]
    public VuetifyCalendarDateValue? Max { get; set; }

    [Parameter]
    public VuetifyCalendarDateValue? Min { get; set; }

    [Parameter]
    public bool ShowAdjacentMonths { get; set; }

    [Parameter]
    public VueStringNumberValue? Year { get; set; }

    [Parameter]
    public VuetifyCalendarWeekdays? Weekdays { get; set; }

    [Parameter]
    public VuetifyCalendarWeeksInMonth? WeeksInMonth { get; set; }

    [Parameter]
    public VueStringNumberValue? FirstDayOfWeek { get; set; }

    [Parameter]
    public bool HideHeader { get; set; }

    [Parameter]
    public bool HideWeekNumber { get; set; }

    [Parameter]
    public EventCallback Next { get; set; }

    [Parameter]
    public EventCallback Prev { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VCalendarHeaderSlotContext>? Header { get; set; }

    [Parameter]
    public RenderFragment<VCalendarEventSlotContext>? EventContent { get; set; }
}
