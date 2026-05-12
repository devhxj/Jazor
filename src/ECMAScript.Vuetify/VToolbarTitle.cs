using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 工具栏标题组件的编写代理。
/// Vuetify toolbar title authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VToolbarTitle")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(TextContent), Name = "text")]
public sealed class VToolbarTitle : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? TextContent { get; set; }
}
