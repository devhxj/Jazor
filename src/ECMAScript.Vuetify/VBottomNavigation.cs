using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VBottomNavigation")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(ActiveChanged), VueEmitKind.ModelUpdate, Name = "update:active")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
/// <summary>
/// Vuetify 底部导航组件。
/// Vuetify bottom navigation component.
/// </summary>
public sealed class VBottomNavigation : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    [Parameter]
    public bool Active { get; set; } = true;

    [Parameter]
    public EventCallback<bool> ActiveChanged { get; set; }

    [Parameter]
    public bool Absolute { get; set; }

    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

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
    public Number? Max { get; set; }

    [Parameter]
    public VuetifyBottomNavigationMode? Mode { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public VueStringNumberValue? Order { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public string? SelectedClass { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
