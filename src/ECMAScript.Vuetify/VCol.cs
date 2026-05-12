using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 网格列组件创作代理。
/// Vuetify grid column component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCol")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VCol : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public string? AlignSelf { get; set; }

    [Parameter]
    public VueStringNumberValue? OrderSm { get; set; }

    [Parameter]
    public VueStringNumberValue? OrderMd { get; set; }

    [Parameter]
    public VueStringNumberValue? OrderLg { get; set; }

    [Parameter]
    public VueStringNumberValue? OrderXl { get; set; }

    [Parameter]
    public VueStringNumberValue? OrderXxl { get; set; }

    [Parameter]
    public VueStringNumberValue? Order { get; set; }

    [Parameter]
    public VueStringNumberValue? OffsetSm { get; set; }

    [Parameter]
    public VueStringNumberValue? OffsetMd { get; set; }

    [Parameter]
    public VueStringNumberValue? OffsetLg { get; set; }

    [Parameter]
    public VueStringNumberValue? OffsetXl { get; set; }

    [Parameter]
    public VueStringNumberValue? OffsetXxl { get; set; }

    [Parameter]
    public VueStringNumberValue? Offset { get; set; }

    [Parameter]
    public VuetifyGridSpanValue? Sm { get; set; }

    [Parameter]
    public VuetifyGridSpanValue? Md { get; set; }

    [Parameter]
    public VuetifyGridSpanValue? Lg { get; set; }

    [Parameter]
    public VuetifyGridSpanValue? Xl { get; set; }

    [Parameter]
    public VuetifyGridSpanValue? Xxl { get; set; }

    [Parameter]
    public VuetifyGridSpanValue? Cols { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
