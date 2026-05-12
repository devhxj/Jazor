using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 视差滚动组件的编写代理。
/// Vuetify parallax authoring proxy for image-backed parallax sections.
/// </summary>
[VueLibraryComponent("vuetify/components", "VParallax")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Placeholder), Name = "placeholder")]
[VueLibrarySlot(nameof(Error), Name = "error")]
[VueLibrarySlot(nameof(Sources), Name = "sources")]
public sealed class VParallax : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VueStringNumberValue? Scale { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Placeholder { get; set; }

    [Parameter]
    public RenderFragment? Error { get; set; }

    [Parameter]
    public RenderFragment? Sources { get; set; }
}
