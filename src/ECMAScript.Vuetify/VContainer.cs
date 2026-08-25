using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 网格容器组件创作代理。
/// Vuetify grid container component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VContainer")]
public sealed class VContainer : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 容器高度。
    /// The height of the container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 容器最大高度。
    /// The maximum height of the container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 容器最大宽度。
    /// The maximum width of the container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 容器最小高度。
    /// The minimum height of the container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 容器最小宽度。
    /// The minimum width of the container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 容器宽度。
    /// The width of the container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 应用的 CSS 类。
    /// The CSS class to apply.
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用的内联样式。
    /// The inline style to apply.
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 是否占满全宽。
    /// Whether to extend to full width.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fluid")]
    public bool Fluid { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Slot for child content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
