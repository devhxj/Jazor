using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 图标创作代理。
/// Vuetify icon authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VIcon")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VIcon : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyIconValue? Icon { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Start { get; set; }

    [Parameter]
    public bool End { get; set; }

    [Parameter]
    public VueStringNumberValue? Opacity { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
