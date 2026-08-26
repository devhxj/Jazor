using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[ECMAScript("vuetify/components", Transform.Component, "VApp")]
/// <summary>
/// Vuetify 应用根包装组件。
/// Vuetify app root wrapper component.
/// </summary>
public sealed class VApp : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 占据全部高度。
    /// Full height.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fullHeight")]
    public bool FullHeight { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
