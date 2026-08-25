using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 主题提供者组件的编写代理。
/// Vuetify theme provider authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VThemeProvider")]
public sealed class VThemeProvider : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 是否显示背景。
    /// Shows background.
    /// </summary>
    [Parameter]
    [ECMAScriptName("withBackground")]
    public bool WithBackground { get; set; }

    /// <summary>
    /// 主题名。
    /// Theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 根标签。
    /// Root element tag.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

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
}
