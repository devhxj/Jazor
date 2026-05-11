using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VBottomNavigation")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(SelectedValueChanged), VueEmitKind.ModelUpdate, Name = "update:selected")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VBottomNavigation : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; } = true;

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyGroupModelValue? SelectedValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> SelectedValueChanged { get; set; }

    [Parameter]
    public string? ActiveColor { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public bool Grow { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    [Parameter]
    public string? Mode { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public bool Shift { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
