using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 实验室垂直步骤条组件的编写代理，基于展开面板工作流。
/// Vuetify labs vertical stepper authoring proxy for expansion-panel based workflows.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VStepperVertical")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Icon), Name = "icon")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(SubtitleContent), Name = "subtitle")]
[VueLibrarySlot(nameof(Prev), Name = "prev")]
[VueLibrarySlot(nameof(Next), Name = "next")]
[VueLibrarySlot(nameof(HeaderItem), Name = "header-item", NamePattern = "header-item.${string}", PatternOnly = true)]
[VueLibrarySlot(nameof(Item), Name = "item", NamePattern = "item.${string}", PatternOnly = true)]
public sealed class VStepperVertical : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public bool Flat { get; set; }

    [Parameter]
    public VuetifyExpansionPanelVariant? Variant { get; set; } = VuetifyExpansionPanelVariant.Accordion;

    [Parameter]
    public int? Max { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public bool Eager { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; } = VuetifyMandatoryMode.Force;

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public bool Focusable { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public VuetifyRippleValue? Ripple { get; set; }

    [Parameter]
    public VuetifyIconValue? CollapseIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? ExpandIcon { get; set; }

    [Parameter]
    public bool HideActions { get; set; }

    [Parameter]
    public VuetifyMobileValue? Mobile { get; set; }

    [Parameter]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; set; }

    [Parameter]
    public bool AltLabels { get; set; }

    [Parameter]
    public VuetifyIconValue? CompleteIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? EditIcon { get; set; }

    [Parameter]
    public bool Editable { get; set; }

    [Parameter]
    public VuetifyIconValue? ErrorIcon { get; set; }

    [Parameter]
    public VuetifyStepperItems? Items { get; set; }

    [Parameter]
    public string? ItemTitle { get; set; }

    [Parameter]
    public string? ItemValue { get; set; }

    [Parameter]
    public bool NonLinear { get; set; }

    [Parameter]
    public string? PrevText { get; set; }

    [Parameter]
    public string? NextText { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VStepperVerticalActionSlotContext>? Actions { get; set; }

    [Parameter]
    public RenderFragment<VStepperVerticalSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VStepperVerticalItemSlotContext>? Icon { get; set; }

    [Parameter]
    public RenderFragment<VStepperVerticalItemSlotContext>? TitleContent { get; set; }

    [Parameter]
    public RenderFragment<VStepperVerticalItemSlotContext>? SubtitleContent { get; set; }

    [Parameter]
    public RenderFragment<VStepperVerticalActionSlotContext>? Prev { get; set; }

    [Parameter]
    public RenderFragment<VStepperVerticalActionSlotContext>? Next { get; set; }

    [Parameter]
    public RenderFragment<VStepperVerticalItemSlotContext>? HeaderItem { get; set; }

    [Parameter]
    public RenderFragment<VStepperVerticalItemSlotContext>? Item { get; set; }
}
