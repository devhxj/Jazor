using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VList")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VList : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifySelectItems? Items { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemTitle { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemValue { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemChildren { get; set; }

    [Parameter]
    public VuetifySelectItemPropsSelector? ItemProps { get; set; }

    [Parameter]
    public string? ItemType { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public string? ActiveColor { get; set; }

    [Parameter]
    public string? ActiveClass { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? ExpandIcon { get; set; }

    [Parameter]
    public string? CollapseIcon { get; set; }

    [Parameter]
    public VuetifyListLines? Lines { get; set; }

    [Parameter]
    public bool Slim { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public bool Nav { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
