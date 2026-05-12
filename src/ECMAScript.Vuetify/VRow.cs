using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VRow")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VRow : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public string? AlignContentSm { get; set; }

    [Parameter]
    public string? AlignContentMd { get; set; }

    [Parameter]
    public string? AlignContentLg { get; set; }

    [Parameter]
    public string? AlignContentXl { get; set; }

    [Parameter]
    public string? AlignContentXxl { get; set; }

    [Parameter]
    public string? AlignContent { get; set; }

    [Parameter]
    public string? JustifySm { get; set; }

    [Parameter]
    public string? JustifyMd { get; set; }

    [Parameter]
    public string? JustifyLg { get; set; }

    [Parameter]
    public string? JustifyXl { get; set; }

    [Parameter]
    public string? JustifyXxl { get; set; }

    [Parameter]
    public string? Justify { get; set; }

    [Parameter]
    public string? AlignSm { get; set; }

    [Parameter]
    public string? AlignMd { get; set; }

    [Parameter]
    public string? AlignLg { get; set; }

    [Parameter]
    public string? AlignXl { get; set; }

    [Parameter]
    public string? AlignXxl { get; set; }

    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public bool NoGutters { get; set; }

    [Parameter]
    public string? Align { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
