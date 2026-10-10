using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 分隔线组件。
/// Vuetify divider component.
/// </summary>
[ECMAScript("vuetify/components/VDivider")]
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
    /// <remarks data-authoring="types">C# 类型为 int?；Razor 数值写 Thickness="@(32)" 或 Thickness="@value"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。</remarks>
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
