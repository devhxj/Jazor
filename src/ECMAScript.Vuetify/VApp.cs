using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VApp")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
/// <summary>
/// Vuetify 应用根包装组件。
/// Vuetify app root wrapper component.
/// </summary>
public sealed class VApp : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 占据全部高度。
    /// Full height.
    /// </summary>
    [Parameter]
    public bool FullHeight { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
