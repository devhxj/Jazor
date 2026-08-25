using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 工具栏组件的编写代理。
/// Vuetify toolbar authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VToolbar")]
public sealed class VToolbar : ComponentBase, IVuetifyComponent
{
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
    /// 圆角。
    /// Border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 移除圆角。
    /// Removes border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 阴影。
    /// Elevation shadow.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

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
    /// 边框。
    /// Border.
    /// </summary>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 绝对定位。
    /// Applies absolute positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("absolute")]
    public bool Absolute { get; set; }

    /// <summary>
    /// 折叠工具栏。
    /// Collapses the toolbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("collapse")]
    public bool Collapse { get; set; }

    /// <summary>
    /// 主题颜色。
    /// Theme color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 工具栏紧凑度。
    /// Toolbar density.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyToolbarDensityValue? Density { get; set; }

    /// <summary>
    /// 扩展。
    /// Extends the toolbar with an extension slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("extended")]
    public bool Extended { get; set; }

    /// <summary>
    /// 扩展高度。
    /// Extension height.
    /// </summary>
    [Parameter]
    [ECMAScriptName("extensionHeight")]
    public VueStringNumberValue? ExtensionHeight { get; set; }

    /// <summary>
    /// 无阴影。
    /// Removes shadow.
    /// </summary>
    [Parameter]
    [ECMAScriptName("flat")]
    public bool Flat { get; set; }

    /// <summary>
    /// 浮动工具栏。
    /// Floating toolbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("floating")]
    public bool Floating { get; set; }

    /// <summary>
    /// 高。
    /// Height.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 背景图片。
    /// Background image URL.
    /// </summary>
    [Parameter]
    [ECMAScriptName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// 标题。
    /// Title text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public string? Title { get; set; }

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
    /// 图片插槽。
    /// Image slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("image")]
    public RenderFragment? ImageContent { get; set; }

    /// <summary>
    /// 前置插槽。
    /// Prepend slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prepend")]
    public RenderFragment? Prepend { get; set; }

    /// <summary>
    /// 后置插槽。
    /// Append slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("append")]
    public RenderFragment? Append { get; set; }

    /// <summary>
    /// 标题内容插槽。
    /// Title content slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// 扩展区插槽。
    /// Extension slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("extension")]
    public RenderFragment? Extension { get; set; }
}
