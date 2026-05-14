using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 图标创作代理。
/// Vuetify icon authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VIcon")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueProp(nameof(CssClass), Name = "class")]
[VueProp(nameof(CssStyle), Name = "style")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class VIcon : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 显示的图标。
    /// Icon to display.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 是否禁用组件。
    /// Disables the component.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否作为前置图标。
    /// Whether to display as a start/prepend icon.
    /// </summary>
    [Parameter]
    public bool Start { get; set; }

    /// <summary>
    /// 是否作为后置图标。
    /// Whether to display as an end/append icon.
    /// </summary>
    [Parameter]
    public bool End { get; set; }

    /// <summary>
    /// 图标的不透明度。
    /// Opacity of the icon.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 自定义 CSS 类。
    /// Custom CSS class(es).
    /// </summary>
    [Parameter]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 自定义行内样式。
    /// Custom inline style(s).
    /// </summary>
    [Parameter]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 组件尺寸。
    /// Component size.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 渲染的根 HTML 元素标签名。
    /// Root HTML element tag name.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

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
