using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 标签页组件的编写代理。
/// Vuetify tab authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VTab")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VTab : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Value { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
