using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 分隔线组件。
/// Vuetify divider component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDivider")]
public sealed class VDivider : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 是否使用缩进样式。
    /// Whether to use inset style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("inset")]
    public bool Inset { get; set; }

    /// <summary>
    /// 分隔线的粗细。
    /// Thickness of the divider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("thickness")]
    public int? Thickness { get; set; }

    /// <summary>
    /// 是否垂直方向显示。
    /// Whether to display vertically.
    /// </summary>
    [Parameter]
    [ECMAScriptName("vertical")]
    public bool Vertical { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
