using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify window authoring proxy for grouped panel navigation.
/// </summary>
[VueLibraryComponent("vuetify/components", "VWindow")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Additional), Name = "additional")]
[VueLibrarySlot(nameof(Prev), Name = "prev")]
[VueLibrarySlot(nameof(Next), Name = "next")]
public sealed class VWindow : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public bool Continuous { get; set; }

    [Parameter]
    public VuetifyIconValue? NextIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? PrevIcon { get; set; }

    [Parameter]
    public bool Reverse { get; set; }

    [Parameter]
    public VuetifyWindowShowArrowsValue? ShowArrows { get; set; }

    [Parameter]
    public VuetifyTouchValue? Touch { get; set; }

    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VWindowSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VWindowSlotContext>? Additional { get; set; }

    [Parameter]
    public RenderFragment<VWindowControlSlotContext>? Prev { get; set; }

    [Parameter]
    public RenderFragment<VWindowControlSlotContext>? Next { get; set; }
}

