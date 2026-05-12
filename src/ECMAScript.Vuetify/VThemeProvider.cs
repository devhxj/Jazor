using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 主题提供者组件的编写代理。
/// Vuetify theme provider authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VThemeProvider")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VThemeProvider : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool WithBackground { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
