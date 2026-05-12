using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 选择控件组件的编写代理。
/// Vuetify selection-control authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSelectionControl")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(LabelContent), Name = "label")]
[VueLibrarySlot(nameof(Input), Name = "input")]
public sealed class VSelectionControl : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Type { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? DefaultsTarget { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VuetifyNullableBoolean? Disabled { get; set; }

    [Parameter]
    public VuetifyNullableBoolean? Readonly { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public bool Inline { get; set; }

    [Parameter]
    public VuetifyNullableBoolean? Multiple { get; set; }

    [Parameter]
    public VuetifyIconValue? FalseIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? TrueIcon { get; set; }

    [Parameter]
    public VuetifyRippleValue? Ripple { get; set; }

    [Parameter]
    public VuetifyValueComparator? ValueComparator { get; set; }

    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyGroupModelValue? Value { get; set; }

    [Parameter]
    public VuetifyGroupModelValue? TrueValue { get; set; }

    [Parameter]
    public VuetifyGroupModelValue? FalseValue { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VSelectionControlDefaultSlotContext>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VSelectionControlLabelSlotContext>? LabelContent { get; set; }

    [Parameter]
    public RenderFragment<VSelectionControlInputSlotContext>? Input { get; set; }
}
