using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VRating")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ItemContent), Name = "item")]
[VueLibrarySlot(nameof(ItemLabel), Name = "item-label")]
public sealed class VRating : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VueStringNumberValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VueStringNumberValue?> ModelValueChanged { get; set; }

    [Parameter]
    public string? ActiveColor { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public VuetifyItemLabelPosition? ItemLabelPosition { get; set; }

    [Parameter]
    public VuetifyMessagesValue? ItemLabels { get; set; }

    [Parameter]
    public string? ItemAriaLabel { get; set; }

    [Parameter]
    public bool HalfIncrements { get; set; }

    [Parameter]
    public VuetifyIconValue? EmptyIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? FullIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? HalfIcon { get; set; }

    [Parameter]
    public VueStringNumberValue? Length { get; set; }

    [Parameter]
    public bool Hover { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VuetifyRippleValue? Ripple { get; set; }

    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VRatingItemSlotContext>? ItemContent { get; set; }

    [Parameter]
    public RenderFragment<VRatingItemLabelSlotContext>? ItemLabel { get; set; }
}
