using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 滑动分组组件的编写代理，用于水平或垂直可滚动的分组内容。
/// Vuetify slide-group authoring proxy for horizontally or vertically scrollable grouped content.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSlideGroup")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Prev), Name = "prev")]
[VueLibrarySlot(nameof(Next), Name = "next")]
public sealed class VSlideGroup : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    [Parameter]
    public int? Max { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyMobileValue? Mobile { get; set; }

    [Parameter]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; set; }

    [Parameter]
    public bool CenterActive { get; set; }

    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    [Parameter]
    public VuetifyIconValue? NextIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? PrevIcon { get; set; }

    [Parameter]
    public VuetifyShowArrowsValue? ShowArrows { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VSlideGroupSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VSlideGroupSlotContext>? Prev { get; set; }

    [Parameter]
    public RenderFragment<VSlideGroupSlotContext>? Next { get; set; }
}
