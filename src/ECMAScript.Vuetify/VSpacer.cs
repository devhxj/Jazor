using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 间距占位组件的编写代理。
/// Vuetify spacer authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSpacer")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueProp(nameof(CssClass), Name = "class")]
[VueProp(nameof(CssStyle), Name = "style")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class VSpacer : ComponentBase, IVueLibraryComponent
{
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
    /// 渲染的根 HTML 元素标签名。
    /// Root HTML element tag name.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

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
