using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 工具栏标题组件的编写代理。
/// Vuetify toolbar title authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VToolbarTitle")]
public sealed class VToolbarTitle : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// CSS类。
    /// CSS class.
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 行内样式。
    /// Inline style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 文本。
    /// Text content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 文本内容插槽。
    /// Text content slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public RenderFragment? TextContent { get; set; }
}
