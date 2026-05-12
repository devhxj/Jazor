using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 实验室时间选择器组件的编写代理。
/// Vuetify labs time-picker authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VTimePicker")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(ViewModeChanged), VueEmitKind.ModelUpdate, Name = "update:viewMode")]
[VueLibraryEmit(nameof(PeriodChanged), VueEmitKind.ModelUpdate, Name = "update:period")]
[VueLibraryEmit(nameof(HourChanged), VueEmitKind.ModelUpdate, Name = "update:hour")]
[VueLibraryEmit(nameof(MinuteChanged), VueEmitKind.ModelUpdate, Name = "update:minute")]
[VueLibraryEmit(nameof(SecondChanged), VueEmitKind.ModelUpdate, Name = "update:second")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
public sealed class VTimePicker : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyTimePickerModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyTimePickerAllowedUnitValue? AllowedHours { get; set; }

    [Parameter]
    public VuetifyTimePickerAllowedUnitValue? AllowedMinutes { get; set; }

    [Parameter]
    public VuetifyTimePickerAllowedUnitValue? AllowedSeconds { get; set; }

    [Parameter]
    public bool AmpmInTitle { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VuetifyTimePickerFormat? Format { get; set; }

    [Parameter]
    public string? Max { get; set; }

    [Parameter]
    public string? Min { get; set; }

    [Parameter]
    public VuetifyTimePickerViewMode? ViewMode { get; set; }

    [Parameter]
    public EventCallback<VuetifyTimePickerViewMode> ViewModeChanged { get; set; }

    [Parameter]
    public EventCallback<VuetifyTimePickerPeriod> PeriodChanged { get; set; }

    [Parameter]
    public EventCallback<Number> HourChanged { get; set; }

    [Parameter]
    public EventCallback<Number> MinuteChanged { get; set; }

    [Parameter]
    public EventCallback<Number> SecondChanged { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public bool Scrollable { get; set; }

    [Parameter]
    public bool UseSeconds { get; set; }

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
    public RenderFragment? TitleContent { get; set; }
}
