using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VChipGroup")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VChipGroup : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public bool CenterActive { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Column { get; set; }

    [Parameter]
    public bool Filter { get; set; }

    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    [Parameter]
    public VueStringNumberValue? Max { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public VuetifyMobileValue? Mobile { get; set; }

    [Parameter]
    public string? NextIcon { get; set; }

    [Parameter]
    public string? PrevIcon { get; set; }

    [Parameter]
    public VuetifyShowArrowsValue? ShowArrows { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyValueComparator? ValueComparator { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
