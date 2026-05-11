using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify carousel authoring proxy for grouped slide navigation.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCarousel")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Prev), Name = "prev")]
[VueLibrarySlot(nameof(Next), Name = "next")]
[VueLibrarySlot(nameof(Item), Name = "item")]
public sealed class VCarousel : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Cycle { get; set; }

    [Parameter]
    public VuetifyIconValue? DelimiterIcon { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public bool HideDelimiters { get; set; }

    [Parameter]
    public bool HideDelimiterBackground { get; set; }

    [Parameter]
    public VueStringNumberValue? Interval { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Progress { get; set; }

    [Parameter]
    public VuetifyCarouselVerticalDelimiters? VerticalDelimiters { get; set; }

    [Parameter]
    public bool Continuous { get; set; } = true;

    [Parameter]
    public VuetifyIconValue? NextIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? PrevIcon { get; set; }

    [Parameter]
    public bool Reverse { get; set; }

    [Parameter]
    public VuetifyWindowShowArrowsValue? ShowArrows { get; set; } = true;

    [Parameter]
    public VuetifyTouchValue? Touch { get; set; }

    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; } = VuetifyMandatoryMode.Force;

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VWindowSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VWindowControlSlotContext>? Prev { get; set; }

    [Parameter]
    public RenderFragment<VWindowControlSlotContext>? Next { get; set; }

    [Parameter]
    public RenderFragment<VCarouselItemSlotContext>? Item { get; set; }
}
