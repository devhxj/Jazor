using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 代码组件创作代理，用于内联或块级代码容器。
/// Vuetify code authoring proxy for inline or block code containers.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCode")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VCode : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Tag { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
