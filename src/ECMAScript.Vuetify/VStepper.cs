using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify stepper authoring proxy for multi-step workflows.
/// </summary>
[VueLibraryComponent("vuetify/components", "VStepper")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
[VueLibrarySlot(nameof(Header), Name = "header")]
[VueLibrarySlot(nameof(HeaderItem), Name = "header-item", NamePattern = "header-item.${string}")]
[VueLibrarySlot(nameof(Icon), Name = "icon")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(SubtitleContent), Name = "subtitle")]
[VueLibrarySlot(nameof(Item), Name = "item", NamePattern = "item.${string}")]
[VueLibrarySlot(nameof(Prev), Name = "prev")]
[VueLibrarySlot(nameof(Next), Name = "next")]
public sealed class VStepper : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public bool AltLabels { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public VuetifyIconValue? CompleteIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? EditIcon { get; set; }

    [Parameter]
    public bool Editable { get; set; }

    [Parameter]
    public VuetifyIconValue? ErrorIcon { get; set; }

    [Parameter]
    public bool HideActions { get; set; }

    [Parameter]
    public VuetifyStepperItems? Items { get; set; }

    [Parameter]
    public string? ItemTitle { get; set; }

    [Parameter]
    public string? ItemValue { get; set; }

    [Parameter]
    public bool NonLinear { get; set; }

    [Parameter]
    public bool Flat { get; set; }

    [Parameter]
    public VuetifyMobileValue? Mobile { get; set; }

    [Parameter]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; } = VuetifyMandatoryMode.Force;

    [Parameter]
    public int? Max { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? PrevText { get; set; }

    [Parameter]
    public string? NextText { get; set; }

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

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VStepperNavigationSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VStepperNavigationSlotContext>? Actions { get; set; }

    [Parameter]
    public RenderFragment<VStepperItemSlotContext>? Header { get; set; }

    [Parameter]
    public RenderFragment<VStepperItemSlotContext>? HeaderItem { get; set; }

    [Parameter]
    public RenderFragment<VStepperItemSlotContext>? Icon { get; set; }

    [Parameter]
    public RenderFragment<VStepperItemSlotContext>? TitleContent { get; set; }

    [Parameter]
    public RenderFragment<VStepperItemSlotContext>? SubtitleContent { get; set; }

    [Parameter]
    public RenderFragment<VStepperContentItemSlotContext>? Item { get; set; }

    [Parameter]
    public RenderFragment<VStepperActionButtonSlotContext>? Prev { get; set; }

    [Parameter]
    public RenderFragment<VStepperActionButtonSlotContext>? Next { get; set; }
}
