using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 工具栏组件的编写代理。
/// Vuetify toolbar authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VToolbar")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(ImageContent), Name = "image")]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(Extension), Name = "extension")]
public sealed class VToolbar : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    [Parameter]
    public bool Absolute { get; set; }

    [Parameter]
    public bool Collapse { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyToolbarDensityValue? Density { get; set; }

    [Parameter]
    public bool Extended { get; set; }

    [Parameter]
    public VueStringNumberValue? ExtensionHeight { get; set; }

    [Parameter]
    public bool Flat { get; set; }

    [Parameter]
    public bool Floating { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public string? Image { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? ImageContent { get; set; }

    [Parameter]
    public RenderFragment? Prepend { get; set; }

    [Parameter]
    public RenderFragment? Append { get; set; }

    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    [Parameter]
    public RenderFragment? Extension { get; set; }
}
